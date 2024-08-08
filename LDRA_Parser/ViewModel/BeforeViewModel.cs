using LDRA_Parser.Model;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using OpenQA.Selenium.DevTools.V124.ServiceWorker;
using System.Windows;

namespace LDRA_Parser.ViewModel
{
    public class BeforeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<BeforeItem> _items;

        public ObservableCollection<BeforeItem> BeforeViewList
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged("BeforeViewList");
            }
        }

        public BeforeViewModel() // Before ViewModel의 생성자
        {
            BeforeViewList = new ObservableCollection<BeforeItem>();
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
                                        BeforeItem item = new BeforeItem(
                                            cells[0].InnerText.Trim(),
                                            cells[1].InnerText.Trim(),
                                            extractedText2.Trim(),
                                            extractedText3?.Trim(),
                                            "Example",
                                            hrefValue
                                        );
                                        BeforeViewList.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("BeforeViewList");
        }

        public void updateBeforeList(List<BeforeItem> beforeit) // 새로운 BeforeItem 객체 목록으로 BeforeViewList를 업데이트하는 메서드
        {
            BeforeViewList = new ObservableCollection<BeforeItem>();
            if (beforeit != null)
            {
                foreach (var beforeItem in beforeit)
                {
                    BeforeViewList.Add(beforeItem);
                }
                OnPropertyChanged("BeforeViewList");
            }
            else
            {
                MessageBox.Show("차이없음");
            }

        }

        
        private string ExtractTextFromScript(HtmlNode cell)
        {
            var scriptNodes = cell.SelectNodes(".//script");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    string scriptContent = scriptNode.InnerText;

                    int startIndex = scriptContent.IndexOf("document.write('") + "document.write('".Length;
                    int endIndex = scriptContent.LastIndexOf("')");
                    if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
                    {
                        string extractedText = scriptContent.Substring(startIndex, endIndex - startIndex);
                        extractedText = extractedText.Replace("\\'", "'").Replace("\\x", "&#x");

                        int lastQuoteIndex = extractedText.LastIndexOf("')");
                        if (lastQuoteIndex >= 0)
                        {
                            extractedText = extractedText.Substring(0, lastQuoteIndex);
                        }

                        int anchorIndex = extractedText.IndexOf("</a>");
                        if (anchorIndex >= 0)
                        {
                            int documentWriteIndex = extractedText.IndexOf("document.write('") + "document.write('".Length;
                            if (documentWriteIndex >= 0 && documentWriteIndex < anchorIndex)
                            {
                                extractedText = extractedText.Substring(documentWriteIndex, anchorIndex - documentWriteIndex);
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
                            Console.WriteLine($"Found hrefValue: {hrefValue}");
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

        public Visibility ShouldShowDetails
        {
            get { return IsSelected ? Visibility.Visible : Visibility.Collapsed; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}