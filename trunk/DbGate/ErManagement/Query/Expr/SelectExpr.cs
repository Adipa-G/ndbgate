using System;

namespace DbGate.ErManagement.Query.Expr
{
    public class SelectExpr : BaseExpr<SelectExpr>
    {
        public SelectExpr Query(ISelectionQuery query, string alias)
        {
            return BaseQuery(query, alias);
        }

        public SelectExpr Sum()
        {
            return BaseSum();
        }

        public SelectExpr Count()
        {
            return BaseCount();
        }

        public SelectExpr CustFunc(string func)
        {
            return BaseCustFunc(func);
        }

        public SelectExpr Field(string field)
        {
            return BaseField(field);
        }

        public SelectExpr Field(string field, string alias)
        {
            return BaseField(field, alias);
        }

        public SelectExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public SelectExpr Field(Type entityType, string field, string alias)
        {
            return BaseField(entityType, field, alias);
        }

        public static SelectExpr Build()
        {
            return new SelectExpr();
        }
    }
}