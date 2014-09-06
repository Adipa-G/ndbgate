using System;

namespace DbGate
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ForeignKeyInfo : Attribute
    {
        public readonly string[] FromFieldMappings;
        public readonly string Name;
        public readonly Type RelatedOjectType;
        public readonly string[] ToFieldMappings;

        public ForeignKeyInfo(string name, Type relatedObjectType, string[] fromFieldMappings,
                              string[] toFieldMappings)
        {
            Name = name;
            RelatedOjectType = relatedObjectType;
            FromFieldMappings = fromFieldMappings;
            ToFieldMappings = toFieldMappings;
        }

        public ReferentialRuleType UpdateRule { get; set; }

        public ReferentialRuleType DeleteRule { get; set; }

        public bool ReverseRelation { get; set; }

        public bool NonIdentifyingRelation { get; set; }

        public FetchStrategy FetchStrategy { get; set; }

        public bool Nullable { get; set; }
    }
}