using LDRA_Parser.Model;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

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

        public AfterViewModel()
        {
            AfterViewList = new ObservableCollection<AfterItem>();
        }

        public void LoadHtmlContent(string filePath)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Load(filePath);

            var tables = htmlDocument.DocumentNode.SelectNodes("//table");

            if (tables != null)
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
                                        AfterItem item = new AfterItem(
                                            cells[0].InnerText.Trim(),
                                            cells[1].InnerText.Trim(),
                                            extractedText2.Trim(),
                                            extractedText3?.Trim()
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

        private bool ContainsHyperlink(HtmlNodeCollection cells)
        {
            foreach (var cell in cells)
            {
                var aNodes = cell.SelectNodes(".//a[@href]");
                if (aNodes != null)
                {
                    foreach (var aNode in aNodes)
                    {
                        string hrefValue = aNode.Attributes["href"].Value;
                        if (Path.GetExtension(hrefValue) == ".htm")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}