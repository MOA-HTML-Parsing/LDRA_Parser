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
    class AfterViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AfterItem> _people;

        public ObservableCollection<AfterItem> AfterViewList
        {
            get { return _people; }
            set
            {
                _people = value;
                OnPropertyChanged("BeforeViewList");
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
                                    AfterItem item = new AfterItem(
                                        cells[0].InnerText.Trim(),
                                        cells[1].InnerText.Trim(),
                                        cells[2].InnerText.Trim(),
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
