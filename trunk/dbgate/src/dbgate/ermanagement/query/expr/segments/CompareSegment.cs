using dbgate.ermanagement.exceptions;

namespace dbgate.ermanagement.query.expr.segments
{
    public class CompareSegment : BaseSegment
    {
        private readonly CompareSegmentMode _mode;
	  		
	  	public CompareSegment(CompareSegmentMode mode)
	  	{
	  		_mode = mode;
	  	}

        public CompareSegmentMode Mode
        {
            get { return _mode; }
        }
	  	
        public override SegmentType SegmentType
        {
            get { return SegmentType.Compare; }
        }

        public ISegment Left { get; set; }

        public ISegment Right { get; set; }
	  	
	  	public override ISegment Add(ISegment segment)
        {
            switch (segment.SegmentType)
            {
                case SegmentType.Field:
                case SegmentType.Value:
                case SegmentType.Query:
                    if (Left == null
                        && !(_mode == CompareSegmentMode.Exists || _mode == CompareSegmentMode.NotExists))
                    {
                        Left = segment;
                    }
                    else
                    {
                        Right = segment;
                    }
                    return this;
                case SegmentType.Group:
                    var groupFunctionSegment = (GroupFunctionSegment) segment;
                    if (groupFunctionSegment.SegmentToGroup == null)
                    {
                        if (Right != null && Right.SegmentType == SegmentType.Field)
                        {
                            groupFunctionSegment.SegmentToGroup = (FieldSegment)Right;
                            Right = groupFunctionSegment;
                        }
                        else if (Left != null && Left.SegmentType == SegmentType.Field)
                        {
                            groupFunctionSegment.SegmentToGroup = (FieldSegment)Left;
                            Left = groupFunctionSegment;
                        }
                    }
                    else
                    {
                        if (Left == null 
                            && !(Mode == CompareSegmentMode.Exists || Mode == CompareSegmentMode.NotExists))
                        {
                            Left = segment;
                        }
                        else
                        {
                            Right = segment;
                        }
                    }
                    return this;
                case SegmentType.Merge:
                    segment.Add(this);
                    Parent = segment;
                    return segment;
                case SegmentType.Compare:
                    throw new ExpressionParsingException("Cannot add compare segment to compare segment");
                default:
                    return this;
            }
        }
    }
}
