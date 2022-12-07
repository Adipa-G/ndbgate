using System;
using System.Linq.Expressions;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QueryGroup
    {
        private static AbstractGroupFactory factory;

        public static AbstractGroupFactory Factory
        {
            set => factory = value;
        }

        public static IQueryGroup RawSql(string sql)
        {
            var queryGroup = (AbstractSqlQueryGroup) factory.CreateGroup(QueryGroupExpressionType.RawSql);
            queryGroup.Sql = sql;
            return queryGroup;
        }

        public static IQueryGroup Field<T>(Expression<Func<T, object>> prop)
        {
            var fieldName = ReflectionUtils.GetPropertyNameFromExpression(prop);
            return Field(typeof(T), fieldName);
        }
        
        public static IQueryGroup Field(Type entityType, string field)
        {
            var expressionGroup = (AbstractExpressionGroup) factory.CreateGroup(QueryGroupExpressionType.Expression);
            expressionGroup.Expr = GroupExpr.Build().Field(entityType, field);
            return expressionGroup;
        }
    }
}