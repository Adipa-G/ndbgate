namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.mappings
{
    public class ReferentialRuleTypeMapItem
    {
        public ReferentialRuleTypeMapItem()
        {
        }

        public ReferentialRuleTypeMapItem(ReferentialRuleType ruleType, string ruleName)
        {
            this.RuleType = ruleType;
            this.RuleName = ruleName;
        }

        public ReferentialRuleType RuleType { get; set; }

        public string RuleName { get; set; }
    }
}
