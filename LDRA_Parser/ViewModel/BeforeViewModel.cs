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
    class BeforeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<BeforeItem> _people;

        public ObservableCollection<BeforeItem> BeforeViewList
        {
            get { return _people; }
            set
            {
                _people = value;
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
                                    BeforeItem item = new BeforeItem(
                                        cells[0].InnerText.Trim(),
                                        cells[1].InnerText.Trim(),
                                        cells[2].InnerText.Trim(),
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
