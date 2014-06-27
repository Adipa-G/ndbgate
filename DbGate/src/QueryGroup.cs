using System;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QueryGroup
    {
        private static AbstractGroupFactory _factory;

        public static AbstractGroupFactory Factory
        {
            set { _factory = value; }
        }

        public static IQueryGroup RawSql(string sql)
        {
            var queryGroup = (AbstractSqlQueryGroup) _factory.CreateGroup(QueryGroupExpressionType.RawSql);
            queryGroup.Sql = sql;
            return queryGroup;
        }

        public static IQueryGroup Field(Type entityType, string field)
        {
            var expressionGroup = (AbstractExpressionGroup) _factory.CreateGroup(QueryGroupExpressionType.Expression);
            expressionGroup.Expr = GroupExpr.Build().Field(entityType, field);
            return expressionGroup;
        }
    }
}