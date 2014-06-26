using System;
using dbgate.exceptions;

namespace dbgate.ermanagement.query.expr.segments
{
    public class FieldSegment : BaseSegment
    {
        private readonly Type _entityType;
        private readonly string _typeAlias;
        private readonly string _field;
        private readonly string _alias;

        public FieldSegment(string field) 
            : this ((Type)null,field)
        {
            _field = field;
        }

        public FieldSegment(string field, string alias)
            : this(null,field,alias)
        {
        }

        public FieldSegment(Type entitytType, string field)
        {
            _entityType = entitytType;
            _field = field;
        }

        public FieldSegment(Type entitytType, string field, string alias)
            : this(entitytType, field)
        {
            _alias = alias;
        }

        public FieldSegment(Type entityType, string typeAlias, string field, string alias)
            : this(entityType, field, alias)
        {
            _typeAlias = typeAlias;
        }

        public override SegmentType SegmentType
        {
            get { return SegmentType.Field; }
        }

        public Type EntityType
        {
            get { return _entityType; }
        }

        public string Field
        {
            get { return _field; }
        }

        public string Alias
        {
            get { return _alias; }
        }

        public string TypeAlias
        {
            get { return _typeAlias; }
        }

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
