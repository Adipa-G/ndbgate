using System;
using System.Collections.Generic;

namespace DbGate
{
    public abstract class AbstractRelation : IRelation
    {
        protected AbstractRelation(String attributeName,
            string relationshipName,
            Type sourceObjectType,
            Type relatedObjectType,
            RelationColumnMapping[] tableColumnMappings)
            : this(attributeName,
                  relationshipName,
                  sourceObjectType,
                  relatedObjectType,
                  tableColumnMappings,
                  ReferentialRuleType.Restrict,
                  ReferentialRuleType.Cascade,
                  false,
                  false,
                  FetchStrategy.Default,
                  false)
        {
        }

        protected AbstractRelation(String attributeName,
            string relationshipName,
            Type sourceObjectType,
            Type relatedObjectType,
            RelationColumnMapping[] tableColumnMappings,
            ReferentialRuleType updateRule,
            ReferentialRuleType deleteRule,
            bool reverseRelationship,
            bool nonIdentifyingRelation,
            FetchStrategy fetchStrategy,
            bool nullable)
        {
            AttributeName = attributeName;
            RelationShipName = relationshipName;
            SourceObjectType = sourceObjectType;
            RelatedObjectType = relatedObjectType;
            TableColumnMappings = tableColumnMappings;
            UpdateRule = updateRule;
            DeleteRule = deleteRule;
            ReverseRelationship = reverseRelationship;
            NonIdentifyingRelation = nonIdentifyingRelation;
            FetchStrategy = fetchStrategy;
            Nullable = nullable;
        }

        #region IRelation Members

        public string AttributeName { get; set; }

        public string RelationShipName { get; set; }

        public Type SourceObjectType { get; set; }

        public Type RelatedObjectType { get; set; }

        public ICollection<RelationColumnMapping> TableColumnMappings { get; set; }

        public ReferentialRuleType UpdateRule { get; set; }

        public ReferentialRuleType DeleteRule { get; set; }

        public bool ReverseRelationship { get; set; }

        public bool NonIdentifyingRelation { get; set; }

        public FetchStrategy FetchStrategy { get; set; }

        public bool Nullable { get; set; }

        public IRelation Clone()
        {
            return (AbstractRelation)MemberwiseClone();
        }
        #endregion
    }
}