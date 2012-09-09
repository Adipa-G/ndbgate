using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace dbgate.ermanagement.query.segments.selection
{
    public class SqlQuerySelection : SqlSegment,IQuerySelection
    {
        public SqlQuerySelection(string sql)
            : base(sql)
        {
        }

        public object Retrieve(IDataReader reader)
        {
            ICollection<String> columns = Sql.Split(new[]{','},StringSplitOptions.RemoveEmptyEntries);
	  	 	
	  	 	Object[] readObjects = new Object[columns.Count];
            int i = 0;
            foreach (string column in columns)
            {
                int columnIndex = reader.GetOrdinal(column);
                Object obj = reader.GetValue(columnIndex);
                readObjects[i++] = obj;    
            }

	  	 	if (readObjects.Length == 0)
	  	 	    return readObjects[0];

	  	 	return readObjects;
        }
    }
}
