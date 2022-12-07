using System;
using System.Linq.Expressions;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QuerySelection
    {
        private static AbstractSelectionFactory factory;

        public static AbstractSelectionFactory Factory
        {
            set => factory = value;
        }

        public static IQuerySelection RawSql(string sql)
        {
            var querySelection =
                (AbstractSqlQuerySelection) factory.CreateSelection(QuerySelectionExpressionType.RawSql);
            querySelection.Sql = sql;
            return querySelection;
        }

        public static IQuerySelection EntityType(Type type)
        {
            var querySelection =
                (AbstractTypeSelection) factory.CreateSelection(QuerySelectionExpressionType.EntityType);
            querySelection.EntityType = type;
            return querySelection;
        }

        public static IQuerySelection EntityType<T>()
        {
            var querySelection =
                (AbstractTypeSelection)factory.CreateSelection(QuerySelectionExpressionType.EntityType);
            querySelection.EntityType = typeof(T);
            return querySelection;
        }

        public static IQuerySelection Query(ISelectionQuery query, String alias)
        {
            var expressionSelection =
                (AbstractExpressionSelection) factory.CreateSelection(QuerySelectionExpressionType.Expression);
            expressionSelection.Expr = SelectExpr.Build().Query(query, alias);
            return expressionSelection;
        }

        private static IQuerySelection Expression(SelectExpr expr)
        {
            var expressionSelection =
                (AbstractExpressionSelection) factory.CreateSelection(QuerySelectionExpressionType.Expression);
            expressionSelection.Expr = expr;
            return expressionSelection;
        }

        public static IQuerySelection Field(String field)
        {
            return Expression(SelectExpr.Build().Field(field));
        }

        public static IQuerySelection Field(String field, String alias)
        {
            return Expression(SelectExpr.Build().Field(field, alias));
        }

        public static IQuerySelection Field(Type entityType, String field, String alias)
        {
            return Expression(SelectExpr.Build().Field(entityType, field, alias));
        }

        public static IQuerySelection Field<T>(Expression<Func<T, object>> prop)
        {
            var entityType = typeof(T);
            var fieldName = ReflectionUtils.GetPropertyNameFromExpression(prop);
            return Expression(SelectExpr.Build().Field(entityType, fieldName));
        }

        public static IQuerySelection Field<T>(Expression<Func<T, object>> prop, String alias)
        {
            var entityType = typeof(T);
            var fieldName = ReflectionUtils.GetPropertyNameFromExpression(prop);
            return Expression(SelectExpr.Build().Field(entityType, fieldName, alias));
        }

        public static IQuerySelection Sum(Type entityType, String field, String alias)
        {
            return Expression(SelectExpr.Build().Field(entityType, field, alias).Sum());
        }

        public static IQuerySelection Sum<T>(Expression<Func<T, object>> prop, String alias)
        {
            return Expression(SelectExpr.Build().Field(prop, alias).Sum());
        }

        public static IQuerySelection Count(Type entityType, String field, String alias)
        {
            return Expression(SelectExpr.Build().Field(entityType, field, alias).Count());
        }

        public static IQuerySelection Count<T>(Expression<Func<T, object>> prop, String alias)
        {
            return Expression(SelectExpr.Build().Field(prop, alias).Count());
        }

        public static IQuerySelection CustFunction(String sqlFunction, Type entityType, String field, String alias)
        {
            return Expression(SelectExpr.Build().Field(entityType, field, alias).CustFunc(sqlFunction));
        }

        public static IQuerySelection CustFunction<T>(String sqlFunction, Expression<Func<T, object>> prop, String alias)
        {
            return Expression(SelectExpr.Build().Field(prop, alias).CustFunc(sqlFunction));
        }
    }
}