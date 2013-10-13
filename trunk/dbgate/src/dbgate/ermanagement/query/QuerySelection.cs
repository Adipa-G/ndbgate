using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
using System;

namespace dbgate.ermanagement.query
{
    public class QuerySelection
    {
		private static AbstractQuerySelectionFactory _factory;

		public static AbstractQuerySelectionFactory Factory
		{
			set { _factory = value;}
		}

        public static IQuerySelection RawSql(string sql)
        {
			AbstractSqlQuerySelection querySelection = (AbstractSqlQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.RAW_SQL);
			querySelection.Sql = sql;
			return querySelection;
        }

		public static IQuerySelection EntityType(Type type)
        {
			AbstractTypeQuerySelection querySelection = (AbstractTypeQuerySelection) _factory.CreateSelection(QuerySelectionExpressionType.ENTITY_TYPE);
			querySelection.EntityType = type;
			return querySelection;
        }
    }
}
