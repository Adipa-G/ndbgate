using System;

namespace dbgate.ermanagement
{
    public class DefaultDbRelation : AbstractDbRelation
    {
        public DefaultDbRelation(string attributeName, string relationshipName,Type relatedObjectType
                                , DbRelationColumnMapping[] tableColumnMappings)
            : base(attributeName, relationshipName, relatedObjectType, tableColumnMappings)
        {
        }

        public DefaultDbRelation(string attributeName, string relationshipName,Type relatedObjectType
            , DbRelationColumnMapping[] tableColumnMappings, ReferentialRuleType updateRule,
            ReferentialRuleType deleteRule, bool reverseRelationship, bool nonIdentifyingRelation,bool lazy) 
            : base(attributeName, relationshipName, relatedObjectType, tableColumnMappings,
            updateRule, deleteRule, reverseRelationship, nonIdentifyingRelation,lazy)
        {
        }
    }
}