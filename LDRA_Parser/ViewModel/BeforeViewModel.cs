using LDRA_Parser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace LDRA_Parser.ViewModel
{
    public class BeforeViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<BeforeItem> item;

        public ObservableCollection<BeforeItem> BeforeViewList
        {
            get { return item; }
            set
            {
                item = value;
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
                                            BeforeItem item = new BeforeItem(
                                                cells[0].InnerText.Trim(),
                                                cells[1].InnerText.Trim(),
                                                extractedText.Trim(),
                                                cells[3].InnerText.Trim()
                                            );
                                            BeforeViewList.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("BeforeViewList");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
