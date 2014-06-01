using System;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.query.expr.segments;

namespace dbgate.ermanagement.query.expr
{
    public class BaseExpr
    {
        protected ISegment _rootSegment;
	  	
	  	protected BaseExpr()
	  	{
	  	}
	  	
	  	public ISegment RootSegment
	  	{
            get { return _rootSegment; }
	  	}
	  	
	  	protected BaseExpr Sum()
	  	{
	  	    var segment = new GroupFunctionSegment(GroupFunctionSegmentType.Sum);
	  	    return Add(segment);
	  	}

        protected BaseExpr Count()
	  	{
	  	    var segment = new GroupFunctionSegment(GroupFunctionSegmentType.Count);
	  	    return Add(segment);
	  	}

        protected BaseExpr CustFunc(string func)
	  	{
	  	    var segment = new GroupFunctionSegment(func);
	  	    return Add(segment);
	  	}

        protected BaseExpr Field(Type entityType, String field)
	  	{
	  	    var segment = new FieldSegment(entityType,field);
	  	    return Add(segment);
	  	}

        protected BaseExpr Field(Type entityType, String field, String alias)
	  	{
            var segment = new FieldSegment(entityType, field, alias);
	  	    return Add(segment);
	  	}
	  	
	  	private BaseExpr Add(ISegment segmentToAdd)
	  	{
	  	    if (_rootSegment == null)
	  	    {
	  	        _rootSegment = segmentToAdd;
	  	        return this;
	  	    }
	  	
	  	    switch (_rootSegment.SegmentType)
	  	    {
	  	        case SegmentType.Field:
	  	            if (segmentToAdd.SegmentType == SegmentType.Group)
	  	            {
	  	                ((GroupFunctionSegment)segmentToAdd).SegmentToGroup = _rootSegment;
	  	                _rootSegment = segmentToAdd;
	  	            }
	  	            else
	  	            {
	  	                throw new ExpressionParsingError(String.Format("Cannot add segment type {0} to field segment",segmentToAdd.SegmentType));
	  	            }
	  	            break;
	  	        case SegmentType.Group:
	  	            if (segmentToAdd.SegmentType == SegmentType.Field)
	  	            {
	  	                ((GroupFunctionSegment)_rootSegment).SegmentToGroup = segmentToAdd;
	  	            }
	  	            else
	  	            {
	  	                throw new ExpressionParsingError(String.Format("Cannot add segment type {0} to group segment",segmentToAdd.SegmentType));
	  	            }
	  	            break;
	  	        case SegmentType.Value:
	  	            throw new ExpressionParsingError(String.Format("Cannot add segment type {0} to value segment",segmentToAdd.SegmentType));
	  	        case SegmentType.Compare:
	  	            ((CompareSegment) _rootSegment).Right = segmentToAdd;
	  	            break;
	  	    }
	  	    return this;
	  	}
    }
}
