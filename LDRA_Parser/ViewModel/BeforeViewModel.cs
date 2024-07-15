using LDRA_Parser.Model;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

        public BeforeViewModel()
        {
            BeforeViewList = new ObservableCollection<BeforeItem>();
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

                                    BeforeItem item = new BeforeItem(
                                        cells[0].InnerText.Trim(),
                                        cells[1].InnerText.Trim(),
                                        extractedText2.Trim(),
                                        extractedText3?.Trim()
                                    );
                                    BeforeViewList.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("BeforeViewList");
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

                        var mainContentStart = extractedText.IndexOf(">") + 1;
                        var mainContentEnd = extractedText.LastIndexOf("</a>");
                        if (mainContentStart >= 0 && mainContentEnd >= 0 && mainContentEnd > mainContentStart)
                        {
                            extractedText = extractedText.Substring(mainContentStart, mainContentEnd - mainContentStart);
                        }

                        return extractedText;
                    }
                }
            }
            return cell.InnerText.Trim();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}