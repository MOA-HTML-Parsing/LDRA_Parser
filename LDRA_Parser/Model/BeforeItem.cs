
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace LDRA_Parser.Model
{
    public class BeforeItem
    {
        public string Number_of_Violations { get; set; }    
        public string LDRA_Code { get; set; }
        public string Rule_Standards { get; set; }
        public string MISRA_Code { get; set; }
        public string Changed_Content { get; set; }

        public string HrefValue { get; set; } //상세 에러(PopUp의 HTML)의 상세주소

        public List<ViolationItem> violationItems { get; set; }


        public BeforeItem(string violations, string code, string standards, string misraCode, string changedContent, string hrefValue)
        {
            Number_of_Violations = violations;
            LDRA_Code = code;
            Rule_Standards = standards;
            MISRA_Code = misraCode;
            Changed_Content = changedContent;
            HrefValue = hrefValue;
            violationItems= new List<ViolationItem>();

        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
