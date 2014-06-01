using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.query.expr.segments
{
    public class CompareSegment : ISegment
    {
        private readonly CompareSegmentType _mode;
	  		
	  	public CompareSegment(CompareSegmentType mode)
	  	{
	  		_mode = mode;
	  	}
	  	
        public SegmentType SegmentType
        {
            get { return SegmentType.Compare; }
        }

        public ISegment Left { get; set; }

        public ISegment Right { get; set; }
	  	
	  	public CompareSegmentType Mode
	  	{
            get { return _mode; }
	  	}
    }
}
