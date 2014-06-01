using System;

namespace dbgate.ermanagement.query.expr
{
    public class SelectExpr : BaseExpr
    {
        public new SelectExpr Sum()
	  	{
	  		return (SelectExpr)base.Sum();
	  	}
	  		
	  	public new SelectExpr Count()
	  	{
	  		return (SelectExpr)base.Count();
	  	}
	  		
	  	public new SelectExpr CustFunc(string func)
	  	{
	  		return (SelectExpr)base.CustFunc(func);
	  	}
	  		
	  	public new SelectExpr Field(Type entityType, string field)
	  	{
	  		return (SelectExpr)base.Field(entityType, field);
	  	}

        public new SelectExpr Field(Type entityType, string field, String alias)
	  	{
	  		return (SelectExpr)base.Field(entityType, field,alias);
	  	}
	  		
	  	public static SelectExpr Build()
	  	{
	  		return new SelectExpr();
	  	}
    }
}
