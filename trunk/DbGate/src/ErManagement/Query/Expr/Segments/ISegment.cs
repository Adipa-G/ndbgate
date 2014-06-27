namespace DbGate.ErManagement.Query.Expr.Segments
{
    public interface ISegment
    {
        SegmentType SegmentType { get; }

        ISegment Add(ISegment segment);
    }
}