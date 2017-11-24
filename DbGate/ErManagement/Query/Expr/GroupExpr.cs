using System;
using System.Linq.Expressions;

namespace DbGate.ErManagement.Query.Expr
{
    public class GroupExpr : BaseExpr<GroupExpr>
    {
        public GroupExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public GroupExpr Field<T>(Expression<Func<T, object>> prop)
        {
            return BaseField(prop);
        }

        
        public static GroupExpr Build()
        {
            return new GroupExpr();
        }
    }
}