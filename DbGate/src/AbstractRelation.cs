using System;
using System.Collections.Generic;

namespace DbGate
{
    public abstract class AbstractRelation : IRelation
    {
        protected AbstractRelation(String attributeName, string relationshipName, Type relatedObjectType
                                   , RelationColumnMapping[] tableColumnMappings)
            : this(attributeName, relationshipName, relatedObjectType, tableColumnMappings, ReferentialRuleType.Restrict
                   , ReferentialRuleType.Cascade, false, false, false,false)
        {
        }

        protected AbstractRelation(String attributeName, string relationshipName, Type relatedObjectType
                                   , RelationColumnMapping[] tableColumnMappings, ReferentialRuleType updateRule
                                   , ReferentialRuleType deleteRule, bool reverseRelationship
                                   , bool nonIdentifyingRelation, bool lazy, bool nullable)
        {
            AttributeName = attributeName;
            RelationShipName = relationshipName;
            RelatedObjectType = relatedObjectType;
            TableColumnMappings = tableColumnMappings;
            UpdateRule = updateRule;
            DeleteRule = deleteRule;
            ReverseRelationship = reverseRelationship;
            NonIdentifyingRelation = nonIdentifyingRelation;
            Lazy = lazy;
            Nullable = nullable;
        }

        #region IRelation Members

        public string AttributeName { get; set; }

        public string RelationShipName { get; set; }

        public Type RelatedObjectType { get; set; }

        public ICollection<RelationColumnMapping> TableColumnMappings { get; set; }

        public ReferentialRuleType UpdateRule { get; set; }

        public ReferentialRuleType DeleteRule { get; set; }

        public bool ReverseRelationship { get; set; }

        public bool NonIdentifyingRelation { get; set; }

        public bool Lazy { get; set; }

        public bool Nullable { get; set; }

        public IRelation Clone()
        {
            return (AbstractRelation)MemberwiseClone();
        }

        #endregion
    }
}