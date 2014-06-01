using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbgate.ermanagement.query.expr.segments
{
    public class GroupFunctionSegment : ISegment
    {
        private readonly GroupFunctionSegmentType _groupFunctionType;
	  	private readonly string _custFunction;
	  	
        public ISegment SegmentToGroup { get; set; }
	  	
	  	public GroupFunctionSegment(GroupFunctionSegmentType groupFunctionType)
	  	{
	  	    _groupFunctionType = groupFunctionType;
	  	    _custFunction = null;
	  	}
	  	
	  	public GroupFunctionSegment(String custFunction)
	  	{
	  	    _groupFunctionType = GroupFunctionSegmentType.CustFunc;
	  	    _custFunction = custFunction;
	  	}
	  	
	  	public SegmentType SegmentType
	  	{
            get { return SegmentType.Group; }
	  	}
	  	
	  	public GroupFunctionSegmentType GroupFunctionType
	  	{
            get { return _groupFunctionType; }
	  	}
	  	
	  	public String CustFunction
	  	{
            get { return _custFunction; }
	  	}
    }
}
