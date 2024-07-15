using LDRA_Parser.Model;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
                                    var scriptNode = cells[2].SelectSingleNode(".//script");
                                    if (scriptNode != null)
                                    {
                                        string scriptContent = scriptNode.InnerText;
                                        int startIndex = scriptContent.IndexOf("document.write('") + "document.write('".Length;
                                        int endIndex = scriptContent.LastIndexOf("')");
                                        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
                                        {
                                            string extractedText = scriptContent.Substring(startIndex, endIndex - startIndex);
                                            extractedText = extractedText.Replace("\\'", "'").Replace("\\x", "&#x");
                                            AfterItem item = new AfterItem(
                                                cells[0].InnerText.Trim(),
                                                cells[1].InnerText.Trim(),
                                                extractedText.Trim(),
                                                cells[3].InnerText.Trim()
                                            );
                                            AfterViewList.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("AfterViewList");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}