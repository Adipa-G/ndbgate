using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group
{
	public class AbstractQueryGroupFactory
	{
		public IAbstractQueryGroup CreateGroup (QueryGroupType groupType)
		{
			switch (groupType) 
			{
				case QueryGroupType.RAW_SQL:
					return new AbstractSqlQueryGroup ();
				default:
					return null;
			}
		}
	}
}

