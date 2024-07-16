using LDRA_Parser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LDRA_Parser.ViewModel
{
    public class ParsedHtmlListModelI : INotifyPropertyChanged
    {
        private ObservableCollection<ViolationItem> _people;

        public ObservableCollection<ViolationItem> ParsedHtmlList
        {
            get { return _people; }
            set
            {
                _people = value;
                OnPropertyChanged("ParsedHtmlList");
            }
        }

        public ParsedHtmlListModelI()
        {
            ParsedHtmlList = new ObservableCollection<ViolationItem>();
        }


        public void updateParsedHtmlList(List<ViolationItem> items)
        {
            ParsedHtmlList = new ObservableCollection<ViolationItem>();
            if (items != null)
            {
                foreach (var updateitem in items)
                {
                    ParsedHtmlList.Add(updateitem);
                }
                OnPropertyChanged("ParsedHtmlList");
            }
            else
            {
                MessageBox.Show("차이없음");
            }

        }


        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
