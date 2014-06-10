using System;

namespace dbgate.ermanagement.query.expr
{
    public class GroupExpr : BaseExpr<GroupExpr>
    {
	  	public GroupExpr Field(Type entityType, string field)
	  	{
	  		return BaseField(entityType, field);
	  	}

        public static GroupExpr Build()
        {
            return new GroupExpr();
        }
    }
}
