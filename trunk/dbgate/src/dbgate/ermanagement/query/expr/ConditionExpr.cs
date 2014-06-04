using System;

namespace dbgate.ermanagement.query.expr
{
    public class ConditionExpr : BaseExpr
    {
        public static ConditionExpr Build()
        {
            return new ConditionExpr();
        }

        public new ConditionExpr Field(Type entityType, string field)
        {
            return (ConditionExpr) base.Field(entityType, field);
        }

        public new ConditionExpr Field(Type entityType,string typeAlias, string field)
        {
            return (ConditionExpr) base.Field(entityType,typeAlias, field,null);
        }

        public new ConditionExpr Value(DbColumnType type, object value)
        {
            return (ConditionExpr) base.Value(type, value);
        }

        public new ConditionExpr Values(DbColumnType type, object[] value)
        {
            return (ConditionExpr) base.Values(type, value);
        }

        public new ConditionExpr Eq()
        {
            return (ConditionExpr) base.Eq();
        }

        public new ConditionExpr Query(ISelectionQuery query)
        {
            return (ConditionExpr)base.Query(query);
        }

        public new ConditionExpr Ge()
        {
            return (ConditionExpr)base.Ge();
        }

        public new ConditionExpr Gt()
        {
            return (ConditionExpr)base.Gt();
        }

        public new ConditionExpr Le()
        {
            return (ConditionExpr)base.Le();
        }

        public new ConditionExpr Lt()
        {
            return (ConditionExpr)base.Lt();
        }

        public new ConditionExpr Neq()
        {
            return (ConditionExpr)base.Neq();
        }

        public new ConditionExpr Like()
        {
            return (ConditionExpr)base.Like();
        }

        public new ConditionExpr Between()
        {
            return (ConditionExpr)base.Between();
        }

        public new ConditionExpr In()
        {
            return (ConditionExpr)base.In();
        }

        public new ConditionExpr Exists()
        {
            return (ConditionExpr)base.Exists();
        }

        public new ConditionExpr NotExists()
        {
            return (ConditionExpr)base.NotExists();
        }

        public new ConditionExpr And()
        {
            return (ConditionExpr)base.And();
        }

        public new ConditionExpr Or()
        {
            return (ConditionExpr)base.Or();
        }

        public ConditionExpr And(ConditionExpr[] expressions)
        {
            return (ConditionExpr)base.And(expressions);
        }

        public ConditionExpr Or(ConditionExpr[] expressions)
        {
            return (ConditionExpr)base.Or(expressions);
        }
    }
}
