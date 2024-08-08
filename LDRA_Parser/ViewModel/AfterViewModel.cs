using LDRA_Parser.Model;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace LDRA_Parser.ViewModel
{
    public class AfterViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AfterItem> _people;

        public ObservableCollection<AfterItem> AfterViewList
        {
            get { return _people; }
            set
            {
                _people = value;
                OnPropertyChanged("AfterViewList");
            }
        }

        private bool _isDetailsVisible;
        public bool IsDetailsVisible
        {
            get { return _isDetailsVisible; }
            set
            {
                _isDetailsVisible = value;
                OnPropertyChanged(nameof(IsDetailsVisible));
            }
        }

        public AfterViewModel() // AfterViewModel의 생성자
        {
            AfterViewList = new ObservableCollection<AfterItem>();
        }

        public void LoadHtmlContent(string filePath, string baseDirectory, string folderName) // HTML 콘텐츠 로드 및 파싱 메소드
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Load(filePath);

            var tables = htmlDocument.DocumentNode.SelectNodes("//table"); // 문서의 모든 table 태그 선택

            if (tables != null) // 테이블이 null이 아니면 각 테이블을 반복해서 돌면서 태그 안의 내용을 파싱해 AfterItem에 저장
            {
                foreach (var table in tables)
                {
                    var thNodes = table.SelectNodes(".//tr/th");
                    if (thNodes != null && thNodes.Count == 4 && thNodes[0].InnerText.Contains("Number of Violations"))
                    {
                        var rows = table.SelectNodes(".//tr");
                        if (rows != null)
                        {
                            foreach (var row in rows)
                            {
                                var cells = row.SelectNodes(".//td");
                                if (cells != null && cells.Count == 4)
                                {
                                    string extractedText2 = ExtractTextFromScript(cells[2]);
                                    string extractedText3 = ExtractTextFromScript(cells[3]);

                                    if (extractedText3 == "&nbsp;")
                                    {
                                        extractedText3 = null;
                                    }
                                    if (ContainsHyperlink(cells))
                                    {
                                        string hrefValue = ExtractHrefValue(cells, baseDirectory, folderName);
                                        AfterItem item = new AfterItem(
                                            cells[0].InnerText.Trim(),
                                            cells[1].InnerText.Trim(),
                                            extractedText2.Trim(),
                                            extractedText3?.Trim(),

                                            "ex",

                                            hrefValue
                                        );
                                        AfterViewList.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("AfterViewList");
        }

        private string ExtractTextFromScript(HtmlNode cell) // 셀의 script태그에서 텍스트를 추출하는 메소드
        {
            var scriptNodes = cell.SelectNodes(".//script");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    string scriptContent = scriptNode.InnerText;

                    int startIndex = scriptContent.IndexOf("document.write('") + "document.write('".Length; // 텍스트의 시작 인덱스
                    int endIndex = scriptContent.LastIndexOf("')"); // 텍스트의 끝 인덱스
                    if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
                    {
                        string extractedText = scriptContent.Substring(startIndex, endIndex - startIndex); // 인덱스 사이의 텍스트를 추출
                        extractedText = extractedText.Replace("\\'", "'").Replace("\\x", "&#x");

                        int lastQuoteIndex = extractedText.LastIndexOf("')"); // 마지막 따옴표 인덱스를 찾음 
                        if (lastQuoteIndex >= 0)
                        {
                            extractedText = extractedText.Substring(0, lastQuoteIndex); // 마지막 따옴표 이전의 텍스트를 추출
                        }

                        int anchorIndex = extractedText.IndexOf("</a>");
                        if (anchorIndex >= 0)
                        {
                            int documentWriteIndex = extractedText.IndexOf("document.write('") + "document.write('".Length; // document.write 인덱스를 찾음
                            if (documentWriteIndex >= 0 && documentWriteIndex < anchorIndex)
                            {
                                extractedText = extractedText.Substring(documentWriteIndex, anchorIndex - documentWriteIndex); // 인덱스 사이의 텍스트를 추출
                            }
                        }

                        return extractedText.Trim();
                    }
                }
            }
            return cell.InnerText.Trim();
        }

        private bool ContainsHyperlink(HtmlNodeCollection cells) // 셀에 하이퍼링크가 포함되어있는지 확인하는 메소드
        {
            foreach (var cell in cells)
            {
                var aNodes = cell.SelectNodes(".//a[@href]"); // href를 가지는 모든 a 태그 선택
                if (aNodes != null)
                {
                    foreach (var aNode in aNodes)
                    {
                        string hrefValue = aNode.Attributes["href"].Value;
                        if (Path.GetExtension(hrefValue) == ".htm") // href 값이 .htm 확장자를 가지고 있는지 확인
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(ShouldShowDetails)); // DetailsTextBox의 Visibility를 업데이트하기 위해 OnPropertyChanged 호출
                }
            }
        }

        private string ExtractHrefValue(HtmlNodeCollection cells, string baseDirectory, string folderName) // 상세 href를 추출하는 메소드
        {

            string targetDirectory = System.IO.Path.Combine(baseDirectory, folderName); // 기본 디렉토리와 폴더 이름을 결합

            foreach (var cell in cells)
            {
                var aNodes = cell.SelectNodes(".//a[@href]"); // href를 가지는 모든 a 태그 선택
                if (aNodes != null)
                {
                    foreach (var aNode in aNodes)
                    {
                        string hrefValue = aNode.Attributes["href"].Value;
                        if (Path.GetExtension(hrefValue) == ".htm") // 확장자가 .htm이면 상세 디렉토리와 상세 html 주소 결합
                        {
                            string absolutePath = System.IO.Path.Combine(targetDirectory, hrefValue); 
                            return absolutePath;
                        }
                    }
                }
            }
            return null;
        }

        public Visibility ShouldShowDetails
        {
            get { return IsSelected ? Visibility.Visible : Visibility.Collapsed; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void updateAfterList(List<AfterItem> afterit) // 새로운 AfterItem 객체 목록으로 AfterViewList를 업데이트하는 메서드
        {
            AfterViewList = new ObservableCollection<AfterItem>();
            if (afterit != null)
            {
                foreach (var afterItem in afterit)
                {
                    AfterViewList.Add(afterItem);
                }
                OnPropertyChanged("AfterViewList");
            }
            else
            {
                MessageBox.Show("차이없음");
            }
        }
    }
}