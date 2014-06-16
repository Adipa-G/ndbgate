using System;

namespace dbgate.ermanagement
{
    public class DefaultRelation : AbstractRelation
    {
        public DefaultRelation(string attributeName, string relationshipName,Type relatedObjectType
                                , RelationColumnMapping[] tableColumnMappings)
            : base(attributeName, relationshipName, relatedObjectType, tableColumnMappings)
        {
        }

        public DefaultRelation(string attributeName, string relationshipName,Type relatedObjectType
            , RelationColumnMapping[] tableColumnMappings, ReferentialRuleType updateRule,
            ReferentialRuleType deleteRule, bool reverseRelationship, bool nonIdentifyingRelation,bool lazy) 
            : base(attributeName, relationshipName, relatedObjectType, tableColumnMappings,
            updateRule, deleteRule, reverseRelationship, nonIdentifyingRelation,lazy)
        {
        }
    }
}