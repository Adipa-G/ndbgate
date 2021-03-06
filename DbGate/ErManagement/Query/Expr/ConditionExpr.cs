using System;
using System.Linq.Expressions;

namespace DbGate.ErManagement.Query.Expr
{
    public class ConditionExpr : BaseExpr<ConditionExpr>
    {
        public static ConditionExpr Build()
        {
            return new ConditionExpr();
        }

        public ConditionExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public ConditionExpr Field(Type entityType, string typeAlias, string field)
        {
            return BaseField(entityType, typeAlias, field, null);
        }

        public ConditionExpr Field<T>(Expression<Func<T, object>> prop)
        {
            return BaseField(prop);
        }

        public ConditionExpr Field<T>(Expression<Func<T, object>> prop, string typeAlias)
        {
            return BaseField(prop, typeAlias,null);
        }

        public ConditionExpr Value(ColumnType type, object value)
        {
            return Values(type, value);
        }

        public ConditionExpr Value(object value)
        {
            return Values( value);
        }

        public ConditionExpr Values(ColumnType type, params object[] value)
        {
            return BaseValues(type, value);
        }

        public ConditionExpr Values(params object[] value)
        {
            return BaseValues(value);
        }

        public ConditionExpr Eq()
        {
            return BaseEq();
        }

        public ConditionExpr Query(ISelectionQuery query)
        {
            return BaseQuery(query);
        }

        public ConditionExpr Ge()
        {
            return BaseGe();
        }

        public ConditionExpr Gt()
        {
            return BaseGt();
        }

        public ConditionExpr Le()
        {
            return BaseLe();
        }

        public ConditionExpr Lt()
        {
            return BaseLt();
        }

        public ConditionExpr Neq()
        {
            return BaseNeq();
        }

        public ConditionExpr Like()
        {
            return BaseLike();
        }

        public ConditionExpr Between()
        {
            return BaseBetween();
        }

        public ConditionExpr In()
        {
            return BaseIn();
        }

        public ConditionExpr Exists()
        {
            return BaseExists();
        }

        public ConditionExpr NotExists()
        {
            return BaseNotExists();
        }

        public ConditionExpr And(params ConditionExpr[] expressions)
        {
            return BaseAnd(expressions);
        }

        public ConditionExpr Or(params ConditionExpr[] expressions)
        {
            return BaseOr(expressions);
        }
    }
}