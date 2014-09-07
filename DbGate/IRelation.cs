using System;
using System.Collections.Generic;

namespace DbGate
{
    public interface IRelation : IField
    {
        string RelationShipName { get; set; }

        Type RelatedObjectType { get; set; }

        ICollection<RelationColumnMapping> TableColumnMappings { get; set; }

        ReferentialRuleType UpdateRule { get; set; }

        ReferentialRuleType DeleteRule { get; set; }

        bool ReverseRelationship { get; set; }

        bool NonIdentifyingRelation { get; set; }

        FetchStrategy FetchStrategy { get; set; }

        bool Nullable { get; set; }

        IRelation Clone();
    }
}