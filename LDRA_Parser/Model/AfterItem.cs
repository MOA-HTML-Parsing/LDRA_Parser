using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDRA_Parser.Model
{
    public class AfterItem
    {
        public string Number_of_Violations { get; set; }
        public string LDRA_Code { get; set; }
        public string Rule_Standards { get; set; }
        public string MISRA_Code { get; set; }

        public string Changed_Content { get; set; }

        public AfterItem(string violations, string code, string standards, string misraCode, string changedContent)
        {
            Number_of_Violations = violations;
            LDRA_Code = code;
            Rule_Standards = standards;
            MISRA_Code = misraCode;
            Changed_Content = changedContent;
        }
    }
}
