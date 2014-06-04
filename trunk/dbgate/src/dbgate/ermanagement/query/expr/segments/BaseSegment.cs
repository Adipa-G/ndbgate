namespace dbgate.ermanagement.query.expr.segments
{
    public abstract class BaseSegment : ISegment
    {
        public ISegment Parent { get; set; }

        public ISegment Active { get; set; }

        public abstract SegmentType SegmentType { get; }

        public abstract ISegment Add(ISegment segment);
    }
}
