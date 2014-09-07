using System;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate
{
    public class QueryJoin
    {
        private static AbstractJoinFactory _factory;

        public static AbstractJoinFactory Factory
        {
            set { _factory = value; }
        }

        public static IQueryJoin RawSql(string sql)
        {
            var queryJoin = (AbstractSqlQueryJoin) _factory.CreateJoin(QueryJoinExpressionType.RawSql);
            queryJoin.Sql = sql;
            return queryJoin;
        }

        public static IQueryJoin EntityType(Type from, Type to)
        {
            return EntityType(from, to, null, null, null);
        }

        public static IQueryJoin EntityType(Type from, Type to, QueryJoinType joinType)
        {
            return EntityType(from, to, null, joinType, null);
        }

        public static IQueryJoin EntityType(Type from, Type to, String alias)
        {
            return EntityType(from, to, null, null, alias);
        }

        public static IQueryJoin EntityType(Type from, Type to, String alias, QueryJoinType joinType)
        {
            return EntityType(from, to, null, joinType, alias);
        }

        public static IQueryJoin EntityType(Type from, Type to, JoinExpr expr)
        {
            return EntityType(from, to, expr, null, null);
        }

        public static IQueryJoin EntityType(Type from, Type to, JoinExpr expr, String alias)
        {
            return EntityType(from, to, expr, null, alias);
        }

        public static IQueryJoin EntityType(Type from, Type to, JoinExpr expr, QueryJoinType? joinType, string alias)
        {
            var typeJoin = (AbstractTypeJoin) _factory.CreateJoin(QueryJoinExpressionType.Type);
            typeJoin.TypeFrom = from;
            typeJoin.TypeTo = to;
            if (expr != null)
            {
                typeJoin.Expr = expr;
            }
            if (joinType.HasValue)
            {
                typeJoin.JoinType = joinType.Value;
            }
            if (!string.IsNullOrEmpty(alias))
            {
                typeJoin.TypeToAlias = alias;
            }
            return typeJoin;
        }
    }
}