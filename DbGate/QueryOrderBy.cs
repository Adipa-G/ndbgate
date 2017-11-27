using System;
using System.Linq.Expressions;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.OrderBy;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QueryOrderBy
    {
        private static AbstractOrderByFactory _factory;

        public static AbstractOrderByFactory Factory
        {
            set { _factory = value; }
        }

        public static IQueryOrderBy RawSql(string sql)
        {
            var queryOrderBy = (AbstractSqlQueryOrderBy) _factory.CreateOrderBy(QueryOrderByExpressionType.RawSql);
            queryOrderBy.Sql = sql;
            return queryOrderBy;
        }

        public static IQueryOrderBy Field<T>(Expression<Func<T, object>> prop)
        {
            return Field(prop, QueryOrderType.Ascend);
        }

        public static IQueryOrderBy Field<T>(Expression<Func<T, object>> prop, QueryOrderType orderType)
        {
            var fieldName = ReflectionUtils.GetPropertyNameFromExpression(prop);
            return Field(typeof(T), fieldName,orderType);
        }

        public static IQueryOrderBy Field(Type type, string field)
        {
            return Field(type, field, QueryOrderType.Ascend);
        }

        public static IQueryOrderBy Field(Type type, string field, QueryOrderType orderType)
        {
            var expressionOrderBy =
                (AbstractExpressionOrderBy) _factory.CreateOrderBy(QueryOrderByExpressionType.Expression);
            expressionOrderBy.Expr = OrderByExpr.Build().Field(type, field);
            expressionOrderBy.OrderType = orderType;
            return expressionOrderBy;
        }
    }
}