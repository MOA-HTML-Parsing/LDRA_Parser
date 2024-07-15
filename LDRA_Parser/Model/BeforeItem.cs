namespace LDRA_Parser.Model
{
    class BeforeItem
    {
        public string Number_of_Violations { get; set; }    
        public string LDRA_Code { get; set; }
        public string Rule_Standards { get; set; }
        public string MISRA_Code { get; set; }

        public BeforeItem(string violations, string code, string standards, string misraCode)
        {
            Number_of_Violations = violations;
            LDRA_Code = code;
            Rule_Standards = standards;
            MISRA_Code = misraCode;
        }
    }
}