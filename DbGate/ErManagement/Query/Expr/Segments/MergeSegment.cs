using System.Collections.Generic;

namespace DbGate.ErManagement.Query.Expr.Segments
{
    public class MergeSegment : BaseSegment
    {
        private readonly MergeSegmentMode mode;
        private readonly ICollection<ISegment> segments;

        public MergeSegment(MergeSegmentMode mode)
        {
            segments = new List<ISegment>();
            this.mode = mode;
        }

        public override SegmentType SegmentType => SegmentType.Merge;

        public MergeSegmentMode Mode => mode;

        public ICollection<ISegment> Segments => segments;

        public void AddSub(ISegment segment)
        {
            segments.Add(segment);
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
                    var result = Active != null ? Active.Add(segment) : segment;
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