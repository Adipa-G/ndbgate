﻿using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QueryGroupCondition
    {
        private static AbstractGroupConditionFactory _factory;

        public static AbstractGroupConditionFactory Factory
        {
            set { _factory = value; }
        }

        public static IQueryGroupCondition RawSql(string sql)
        {
            var queryGroupCondition =
                (AbstractSqlQueryGroupCondition) _factory.CreateGroupCondition(QueryGroupConditionExpressionType.RawSql);
            queryGroupCondition.Sql = sql;
            return queryGroupCondition;
        }

        public static IQueryGroupCondition Expression(GroupConditionExpr expr)
        {
            var expressionGroupCondition =
                (AbstractExpressionGroupCondition)
                _factory.CreateGroupCondition(QueryGroupConditionExpressionType.Expression);
            expressionGroupCondition.Expr = expr;
            return expressionGroupCondition;
        }
    }
}