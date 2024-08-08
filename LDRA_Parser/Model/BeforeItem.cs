
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LDRA_Parser.Model
{
    public class BeforeItem
    {
        public string Number_of_Violations { get; set; } // LDRA .rps 파일의 표 칼럼에 해당하는 변수
        public string LDRA_Code { get; set; } // LDRA .rps 파일의 표 칼럼에 해당하는 변수
        public string Rule_Standards { get; set; } // LDRA .rps 파일의 표 칼럼에 해당하는 변수
        public string MISRA_Code { get; set; } // LDRA .rps 파일의 표 칼럼에 해당하는 변수
        public string Changed_Content { get; set; } // Before & After 비교 내용을 저장하는 변수 (현재는 사용하지 않음)

        public string HrefValue { get; set; } //상세 에러(PopUp의 HTML)의 상세주소

        public List<ViolationItem> violationItems { get; set; } // 표의 각 행의 세부 내용을 저장할 리스트


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
