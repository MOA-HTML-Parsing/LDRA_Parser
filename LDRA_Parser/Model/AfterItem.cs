using System.ComponentModel;

namespace LDRA_Parser.Model
{
    public class AfterItem
    {
        public string Number_of_Violations { get; set; } // 칼럼에 해당하는 데이터를 담는 변수
        public string LDRA_Code { get; set; } // 칼럼에 해당하는 데이터를 담는 변수
        public string Rule_Standards { get; set; } // 칼럼에 해당하는 데이터를 담는 변수
        public string MISRA_Code { get; set; } // 칼럼에 해당하는 데이터를 담는 변수
        public string Changed_Content { get; set; } // Before & After 파일의 차이점을 담는 변수 (현재는 사용하고있지 않음)

        public string HrefValue { get; set; }//상세 에러(PopUp의 HTML)의 상세주소

        public List<ViolationItem> violationItems { get; set; } // 문서 비교 후 변경된 내용을 담는 리스트

        public AfterItem(string violations, string code, string standards, string misraCode, string changedContent, string hrefValue)
        {
            Number_of_Violations = violations;
            LDRA_Code = code;
            Rule_Standards = standards;
            MISRA_Code = misraCode;
            HrefValue = hrefValue;
            violationItems = new List<ViolationItem>(); 
            Changed_Content = changedContent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        }
    }
}
