using System;
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
	  	
        protected BaseExpr Field(Type entityType, String field)
        {
            var segment = new FieldSegment(entityType,field);
            return AddSegment(segment);
        }

        protected BaseExpr Field(Type entityType, String field, String alias)
        {
            var segment = new FieldSegment(entityType,field,alias);
            return AddSegment(segment);
        }
    
        protected BaseExpr Field(Type entityType,String typeAlias,String field,String alias)
        {
            var segment = new FieldSegment(entityType,typeAlias,field,alias);
            return AddSegment(segment);
        }
    
        protected BaseExpr Value(DbColumnType type,Object value)
        {
            var segment = new ValueSegment(type,value);
            return AddSegment(segment);
        }
    
        protected BaseExpr Values(DbColumnType type,Object[] values)
        {
            var segment = new ValueSegment(type,values);
            return AddSegment(segment);
        }
    
        protected BaseExpr Query(ISelectionQuery query)
        {
            return Query(query,null);
        }
    
        protected BaseExpr Query(ISelectionQuery query,String alias)
        {
            var segment = new QuerySegment(query,alias);
            return AddSegment(segment);
        }
    
        protected BaseExpr Sum()
        {
            var segment = new GroupFunctionSegment(GroupFunctionSegmentMode.Sum);
            return AddSegment(segment);
        }
    
        protected BaseExpr Count()
        {
            var segment = new GroupFunctionSegment(GroupFunctionSegmentMode.Count);
            return AddSegment(segment);
        }
    
        protected BaseExpr CustFunc(String func)
        {
            var segment = new GroupFunctionSegment(func);
            return AddSegment(segment);
        }
    
        protected BaseExpr Eq()
        {
            var segment = new CompareSegment(CompareSegmentMode.Eq);
            return AddSegment(segment);
        }
    
        protected BaseExpr Ge()
        {
            var segment = new CompareSegment(CompareSegmentMode.Ge);
            return AddSegment(segment);
        }

        protected BaseExpr Gt()
        {
            var segment = new CompareSegment(CompareSegmentMode.Gt);
            return AddSegment(segment);
        }
    
        protected BaseExpr Le()
        {
            var segment = new CompareSegment(CompareSegmentMode.Le);
            return AddSegment(segment);
        }

        protected BaseExpr Lt()
        {
            var segment = new CompareSegment(CompareSegmentMode.Lt);
            return AddSegment(segment);
        }
    
        protected BaseExpr Neq()
        {
            var segment = new CompareSegment(CompareSegmentMode.Neq);
            return AddSegment(segment);
        }
	  	
        protected BaseExpr Like()
        {
            var segment = new CompareSegment(CompareSegmentMode.Like);
            return AddSegment(segment);
        }

        protected BaseExpr Between()
        {
            var segment = new CompareSegment(CompareSegmentMode.Between);
            return AddSegment(segment);
        }

        protected BaseExpr In()
        {
            var segment = new CompareSegment(CompareSegmentMode.In);
            return AddSegment(segment);
        }

        protected BaseExpr Exists()
        {
            var segment = new CompareSegment(CompareSegmentMode.Exists);
            return AddSegment(segment);
        }

        protected BaseExpr NotExists()
        {
            var segment = new CompareSegment(CompareSegmentMode.NotExists);
            return AddSegment(segment);
        }

        protected BaseExpr And()
        {
            var mergeSegment = new MergeSegment(MergeSegmentMode.And);
            return AddSegment(mergeSegment);
        }

        protected BaseExpr Or()
        {
            var mergeSegment = new MergeSegment(MergeSegmentMode.Or);
            return AddSegment(mergeSegment);
        }

        protected BaseExpr And(BaseExpr[] expressions)
        {
            var mergeSegment = new MergeSegment(MergeSegmentMode.ParaAnd);
            foreach (BaseExpr expression in expressions)
            {
                mergeSegment.AddSub(expression.RootSegment);
            }
            return AddSegment(mergeSegment);
        }

        protected BaseExpr Or(BaseExpr[] expressions)
        {
            var mergeSegment = new MergeSegment(MergeSegmentMode.ParaOr);
            foreach (BaseExpr expression in expressions)
            {
                mergeSegment.AddSub(expression.RootSegment);
            }
            return AddSegment(mergeSegment);
        }

        private BaseExpr AddSegment(ISegment segment)
        {
            _rootSegment = _rootSegment == null
                               ? segment
                               : _rootSegment.Add(segment);
            return this;
        }
    }
}
