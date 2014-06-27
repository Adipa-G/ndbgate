using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DbGate.ErManagement.Query;
using DbGate.Exceptions;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection
{
    public class AbstractSqlQuerySelection : IAbstractSelection
    {
        public string Sql { get; set; }

        #region IAbstractSelection Members

        public QuerySelectionExpressionType SelectionType
        {
            get { return QuerySelectionExpressionType.RawSql; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        public Object Retrieve(IDataReader rs, IDbConnection con, QueryBuildInfo buildInfo)
        {
            try
            {
                IList<String> columns = new List<string>();

                string[] segments = Regex.Split(Sql, "\\s*,\\s*");
                foreach (string segment in segments)
                {
                    if (segment.Trim().Length == 0)
                        continue;
                    columns.Add(segment.Trim());
                }

                var readObjects = new Object[columns.Count];
                for (int i = 0, columnsLength = columns.Count; i < columnsLength; i++)
                {
                    String column = columns[i].ToLowerInvariant();
                    if (column.Contains(" as "))
                    {
                        column = column.Split(new[] {"as"}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    }
                    int ordinal = rs.GetOrdinal(column);
                    Object obj = rs.GetValue(ordinal);
                    readObjects[i] = obj;
                }

                if (readObjects.Length == 1)
                    return readObjects[0];
                return readObjects;
            }
            catch (Exception ex)
            {
                throw new RetrievalException(ex.Message, ex);
            }
        }

        #endregion
    }
}