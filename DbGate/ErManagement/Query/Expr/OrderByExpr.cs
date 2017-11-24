using System;
using System.Linq.Expressions;

namespace DbGate.ErManagement.Query.Expr
{
    public class OrderByExpr : BaseExpr<OrderByExpr>
    {
        public OrderByExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public OrderByExpr Field<T>(Expression<Func<T, object>> prop)
        {
            return BaseField(prop);
        }
        
        public static OrderByExpr Build()
        {
            return new OrderByExpr();
        }
    }
}