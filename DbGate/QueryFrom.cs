using System;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From;
using DbGate.ErManagement.Query;

namespace DbGate
{
    public class QueryFrom
    {
        private static AbstractFromFactory factory;

        public static AbstractFromFactory Factory
        {
            set => factory = value;
        }

        public static IQueryFrom RawSql(string sql)
        {
            var queryFrom = (AbstractSqlQueryFrom) factory.CreateFrom(QueryFromExpressionType.RawSql);
            queryFrom.Sql = sql;
            return queryFrom;
        }

        public static IQueryFrom EntityType(Type entityType)
        {
            var typeFrom = (AbstractTypeFrom) factory.CreateFrom(QueryFromExpressionType.EntityType);
            typeFrom.EntityType = entityType;
            return typeFrom;
        }

        public static IQueryFrom EntityType(Type entityType, String alias)
        {
            var typeFrom = (AbstractTypeFrom) factory.CreateFrom(QueryFromExpressionType.EntityType);
            typeFrom.EntityType = entityType;
            if (!string.IsNullOrEmpty(alias))
            {
                typeFrom.Alias = alias;
            }
            return typeFrom;
        }

        public static IQueryFrom EntityType<T>()
        {
            var typeFrom = (AbstractTypeFrom)factory.CreateFrom(QueryFromExpressionType.EntityType);
            typeFrom.EntityType = typeof(T);
            return typeFrom;
        }

        public static IQueryFrom EntityType<T>(String alias)
        {
            var typeFrom = (AbstractTypeFrom)factory.CreateFrom(QueryFromExpressionType.EntityType);
            typeFrom.EntityType = typeof(T);
            if (!string.IsNullOrEmpty(alias))
            {
                typeFrom.Alias = alias;
            }
            return typeFrom;
        }

        public static IQueryFrom Query(ISelectionQuery query)
        {
            return Query(query, null);
        }

        public static IQueryFrom Query(ISelectionQuery query, String alias)
        {
            var queryFromSub = (AbstractSubQueryFrom) factory.CreateFrom(QueryFromExpressionType.Query);
            queryFromSub.Query = query;
            ;
            if (!string.IsNullOrEmpty(alias))
            {
                queryFromSub.Alias = alias;
            }
            return queryFromSub;
        }

        public static IQueryFrom QueryUnion(bool all, ISelectionQuery[] queries)
        {
            var queryFrom = (AbstractUnionFrom) factory.CreateFrom(QueryFromExpressionType.QueryUnion);
            queryFrom.Queries = queries;
            queryFrom.All = all;
            return queryFrom;
        }
    }
}