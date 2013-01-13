using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractSqlQueryGroup : IAbstractQueryGroup
	{
		public String Sql { get; set; }

		public QueryGroupType GroupType
		{
			get {return QueryGroupType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

