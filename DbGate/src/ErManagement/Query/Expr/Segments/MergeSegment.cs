using System.Collections.Generic;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class MergeSegment : BaseSegment
    {
        private readonly MergeSegmentMode _mode;
        private readonly ICollection<ISegment> _segments;

        public MergeSegment(MergeSegmentMode mode)
        {
            _segments = new List<ISegment>();
            _mode = mode;
        }

        public override SegmentType SegmentType
        {
            get { return SegmentType.Merge; }
        }

        public MergeSegmentMode Mode
        {
            get { return _mode; }
        }

        public ICollection<ISegment> Segments
        {
            get { return _segments; }
        }

        public void AddSub(ISegment segment)
        {
            _segments.Add(segment);
        }

        public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                case SegmentType.Group:
                case SegmentType.Compare:
                    ISegment result = Active != null ? Active.Add(segment) : segment;
                    if (result.SegmentType == SegmentType.Compare
                        && ((CompareSegment) result).Right != null)
                    {
                        AddSub(result);
                        ((CompareSegment) result).Parent = this;
                        Active = null;
                    }
                    else
                    {
                        Active = result;
                    }
                    return this;
                case SegmentType.Merge:
                    var mergeSegment = (MergeSegment) segment;
                    mergeSegment.AddSub(this);
                    Parent = mergeSegment;
                    return mergeSegment;
                default:
                    return this;
            }
        }
    }
}