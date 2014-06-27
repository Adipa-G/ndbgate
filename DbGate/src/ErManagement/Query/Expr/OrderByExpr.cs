using System;

namespace DbGate.ErManagement.Query.Expr
{
    public class OrderByExpr : BaseExpr<OrderByExpr>
    {
        public OrderByExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public static OrderByExpr Build()
        {
            return new OrderByExpr();
        }
    }
}