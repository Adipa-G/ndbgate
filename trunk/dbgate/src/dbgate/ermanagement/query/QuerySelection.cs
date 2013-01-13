using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;

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
			AbstractSqlQuerySelection querySelection = (AbstractSqlQuerySelection) _factory.CreateSelection(QuerySelectionType.RAW_SQL);
			querySelection.Sql = sql;
			return querySelection;
        }
    }
}
