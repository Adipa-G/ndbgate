using System;

namespace dbgate.ermanagement
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple = true)]
    public class ForeignKeyInfo : Attribute
    {
        public readonly string [] FromColumnMappings;
        public readonly string [] ToColumnMappings;
        public readonly string Name;
        public readonly Type RelatedOjectType;

        public ForeignKeyInfo(string name,Type relatedObjectType, string[] fromColumnMappings, string[] toColumnMappings)
        {
            Name = name;
            RelatedOjectType = relatedObjectType;
            FromColumnMappings = fromColumnMappings;
            ToColumnMappings = toColumnMappings;
        }

        public ReferentialRuleType UpdateRule { get; set; }

        public ReferentialRuleType DeleteRule { get; set; }

        public bool ReverseRelation { get; set; }

        public bool NonIdentifyingRelation { get; set; }

        public bool Lazy { get; set; }
    }
}