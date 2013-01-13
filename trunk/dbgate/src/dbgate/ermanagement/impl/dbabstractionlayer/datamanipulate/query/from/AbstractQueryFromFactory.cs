using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from
{
	public class AbstractQueryFromFactory
	{
		public IAbstractQueryFrom CreateFrom (QueryFromType fromType)
		{
			switch (fromType) 
			{
				case QueryFromType.RAW_SQL:
					return new AbstractSqlQueryFrom ();
				default:
					return null;
			}
		}
	}
}

