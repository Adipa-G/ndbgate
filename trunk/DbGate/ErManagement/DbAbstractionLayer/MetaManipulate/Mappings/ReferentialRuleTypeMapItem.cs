namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Mappings
{
    public class ReferentialRuleTypeMapItem
    {
        public ReferentialRuleTypeMapItem()
        {
        }

        public ReferentialRuleTypeMapItem(ReferentialRuleType ruleType, string ruleName)
        {
            RuleType = ruleType;
            RuleName = ruleName;
        }

        public ReferentialRuleType RuleType { get; set; }

        public string RuleName { get; set; }
    }
}