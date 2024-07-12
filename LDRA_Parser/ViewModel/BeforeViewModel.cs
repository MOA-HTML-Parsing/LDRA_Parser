using LDRA_Parser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Sample data
            BeforeViewList = new ObservableCollection<BeforeItem>
        {
            new BeforeItem { Number_of_Violations = "6", LDRA_Code = "36 S", Rule_Standards = "Function has no return statement.", MISRA_Code = "MISRA-C:2004 16.8"},
            new BeforeItem { Number_of_Violations = "0", LDRA_Code = "37 S", Rule_Standards = "Function has no return statement.", MISRA_Code = "MISRA-C:2004 16.8"},
            new BeforeItem { Number_of_Violations = "0", LDRA_Code = "38 S", Rule_Standards = "Function has no return statement.", MISRA_Code = "MISRA-C:2004 16.8"},
            new BeforeItem { Number_of_Violations = "0", LDRA_Code = "40 S", Rule_Standards = "Function has no return statement.", MISRA_Code = "MISRA-C:2004 16.8"}
        };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
