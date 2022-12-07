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

        public QuerySelectionExpressionType SelectionType => QuerySelectionExpressionType.RawSql;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }

        public Object Retrieve(IDataReader rs, ITransaction tx, QueryBuildInfo buildInfo)
        {
            try
            {
                IList<String> columns = new List<string>();

                var segments = Regex.Split(Sql, "\\s*,\\s*");
                foreach (var segment in segments)
                {
                    if (segment.Trim().Length == 0)
                        continue;
                    columns.Add(segment.Trim());
                }

                var readObjects = new Object[columns.Count];
                for (int i = 0, columnsLength = columns.Count; i < columnsLength; i++)
                {
                    var column = columns[i].ToLowerInvariant();
                    if (column.Contains(" as "))
                    {
                        column = column.Split(new[] {"as"}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    }
                    var ordinal = rs.GetOrdinal(column);
                    var obj = rs.GetValue(ordinal);
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