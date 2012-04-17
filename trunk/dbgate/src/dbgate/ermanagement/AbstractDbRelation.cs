using System;
using System.Collections.Generic;

namespace dbgate.ermanagement
{
    public abstract class AbstractDbRelation : IDbRelation
    {
        protected AbstractDbRelation(String attributeName, String relationshipName,Type relatedObjectType
                    , DbRelationColumnMapping[] tableColumnMappings)
            : this(attributeName, relationshipName, relatedObjectType, tableColumnMappings,ReferentialRuleType.Restrict
                    , ReferentialRuleType.Cascade,false,false,false)
        {
        }

        protected AbstractDbRelation(String attributeName, String relationshipName, Type relatedObjectType
                                    , DbRelationColumnMapping[] tableColumnMappings,ReferentialRuleType updateRule
                                    , ReferentialRuleType deleteRule,bool reverseRelationship
                                    ,bool nonIdentifyingRelation,bool lazy)
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
        }

        #region IDbRelation Members

        public string AttributeName { get; set; }

        public string RelationShipName { get; set; }

        public Type RelatedObjectType { get; set; }

        public ICollection<DbRelationColumnMapping> TableColumnMappings { get; set; }

        public ReferentialRuleType UpdateRule { get; set; }

        public ReferentialRuleType DeleteRule { get; set; }

        public bool ReverseRelationship { get; set; }

        public bool NonIdentifyingRelation { get; set; }

        public bool Lazy { get; set; }

        #endregion
    }
}