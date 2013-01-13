using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractSqlQueryFrom : IAbstractQueryFrom
	{
		public String Sql { get; set; }

		public QueryFromType FromType
		{
			get {return QueryFromType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}
	}
}

