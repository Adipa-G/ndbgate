﻿using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition;

namespace dbgate.ermanagement.query
{
    public class QueryCondition
    {
		private static AbstractConditionFactory _factory;

		public static AbstractConditionFactory Factory
		{
			set { _factory = value;}
		}

        public static IQueryCondition RawSql(string sql)
        {
			AbstractSqlQueryCondition queryCondition = (AbstractSqlQueryCondition) _factory.CreateCondition(QueryConditionExpressionType.RAW_SQL);
			queryCondition.Sql = sql;
			return queryCondition;
        }
    }
}
