using System;
using DbGate.Exceptions;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class FieldSegment : BaseSegment
    {
        private readonly string alias;
        private readonly Type entityType;
        private readonly string field;
        private readonly string typeAlias;

        public FieldSegment(string field)
            : this((Type) null, field)
        {
            this.field = field;
        }

        public FieldSegment(string field, string alias)
            : this(null, field, alias)
        {
        }

        public FieldSegment(Type entitytType, string field)
        {
            entityType = entitytType;
            this.field = field;
        }

        public FieldSegment(Type entitytType, string field, string alias)
            : this(entitytType, field)
        {
            this.alias = alias;
        }

        public FieldSegment(Type entityType, string typeAlias, string field, string alias)
            : this(entityType, field, alias)
        {
            this.typeAlias = typeAlias;
        }

        public override SegmentType SegmentType => SegmentType.Field;

        public Type EntityType => entityType;

        public string Field => field;

        public string Alias => alias;

        public string TypeAlias => typeAlias;

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                    throw new ExpressionParsingException("Cannot add field/value/query segments to field segment");
                case SegmentType.Merge:
                    segment.Add(this);
                    return segment;
                case SegmentType.Group:
                case SegmentType.Compare:
                    segment.Add(this);
                    Parent = segment;
                    return segment;
                default:
                    return this;
            }
        }
    }
}