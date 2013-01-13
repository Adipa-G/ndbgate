using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractSqlQuerySelection : IAbstractQuerySelection
	{
		public String Sql { get; set; }

		public QuerySelectionType SelectionType
		{
			get {return QuerySelectionType.RAW_SQL;}
		}
		
		public String CreateSql()
		{
			return Sql;
		}

		public Object Retrieve (IDataReader rs)
		{
			IList<String> columns = new List<string> ();

			var segments = Regex.Split(Sql,"\\s*,\\s*");
			foreach (var segment in segments) 
			{
				if (segment.Trim().Length == 0)
					continue;
				columns.Add(segment.Trim());
			}
	 	
			Object[] readObjects = new Object[columns.Count];
			for (int i = 0, columnsLength = columns.Count; i < columnsLength; i++)
			{
				String column = columns [i].ToLowerInvariant ();
				if (column.Contains (" as ")) 
				{
					column = column.Split(new string[]{"as"},StringSplitOptions.RemoveEmptyEntries) [1].Trim();
				}
				int ordinal = rs.GetOrdinal(column);
				Object obj = rs.GetValue(ordinal);
				readObjects [i] = obj;
			}
	 	
			if (readObjects.Length == 0)
				return readObjects [0];
			return readObjects;
		}
	}
}

