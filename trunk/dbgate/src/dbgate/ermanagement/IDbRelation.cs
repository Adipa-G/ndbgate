using System;
using System.Collections.Generic;

namespace dbgate.ermanagement
{
    public interface IDbRelation : IField
    {
        string RelationShipName { get; set; }

        Type RelatedObjectType { get; set; }

        ICollection<DbRelationColumnMapping> TableColumnMappings { get; set; }

        ReferentialRuleType UpdateRule { get; set; }

        ReferentialRuleType DeleteRule { get; set; }

        bool ReverseRelationship { get; set; }

        bool NonIdentifyingRelation { get; set; }

        bool Lazy { get; set; }
    }
}