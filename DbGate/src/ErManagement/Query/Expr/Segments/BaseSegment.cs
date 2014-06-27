namespace DbGate.ErManagement.Query.Expr.Segments
{
    public abstract class BaseSegment : ISegment
    {
        public ISegment Parent { get; set; }

        public ISegment Active { get; set; }

        #region ISegment Members

        public abstract SegmentType SegmentType { get; }

        public abstract ISegment Add(ISegment segment);

        #endregion
    }
}