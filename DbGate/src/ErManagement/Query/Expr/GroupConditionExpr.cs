using System;

namespace DbGate.ErManagement.Query.Expr
{
    public class GroupConditionExpr : BaseExpr<GroupConditionExpr>
    {
        public static GroupConditionExpr Build()
        {
            return new GroupConditionExpr();
        }

        public GroupConditionExpr Field(Type entityType, string field)
        {
            return BaseField(entityType, field);
        }

        public GroupConditionExpr Field(Type entityType, string typeAlias, string field)
        {
            return BaseField(entityType, typeAlias, field, null);
        }

        public GroupConditionExpr Value(ColumnType type, object value)
        {
            return Values(type, value);
        }

        public GroupConditionExpr Values(ColumnType type, params object[] value)
        {
            return BaseValues(type, value);
        }

        public GroupConditionExpr Sum()
        {
            return BaseSum();
        }

        public GroupConditionExpr Count()
        {
            return BaseCount();
        }

        public GroupConditionExpr CustFunc(string func)
        {
            return BaseCustFunc(func);
        }

        public GroupConditionExpr Eq()
        {
            return BaseEq();
        }

        public GroupConditionExpr Ge()
        {
            return BaseGe();
        }

        public GroupConditionExpr Gt()
        {
            return BaseGt();
        }

        public GroupConditionExpr Le()
        {
            return BaseLe();
        }

        public GroupConditionExpr Lt()
        {
            return BaseLt();
        }

        public GroupConditionExpr Neq()
        {
            return BaseNeq();
        }

        public GroupConditionExpr Between()
        {
            return BaseBetween();
        }

        public GroupConditionExpr In()
        {
            return BaseIn();
        }

        public GroupConditionExpr And(params GroupConditionExpr[] expressions)
        {
            return BaseAnd(expressions);
        }

        public GroupConditionExpr Or(params GroupConditionExpr[] expressions)
        {
            return BaseOr(expressions);
        }
    }
}