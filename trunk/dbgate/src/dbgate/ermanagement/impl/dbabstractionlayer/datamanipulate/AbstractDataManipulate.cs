using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.impl.utils;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate
{
    public class AbstractDataManipulate : IDataManipulate
    {
        private IDbLayer _dbLayer;

        public AbstractDataManipulate(IDbLayer dbLayer)
        {
            _dbLayer = dbLayer;
        }

        #region IDataManipulate Members

        public string CreateLoadQuery(string tableName, ICollection<IDbColumn> dbColumns)
        {
            var keys = new List<IDbColumn>();

            foreach (IDbColumn dbColumn in dbColumns)
            {
                if (dbColumn.Key)
                {
                    keys.Add(dbColumn);
                }
            }

            var sb = new StringBuilder();
            sb.Append("SELECT * FROM ");
            sb.Append(tableName);
            sb.Append(" WHERE ");

            for (int i = 0; i < keys.Count; i++)
            {
                IDbColumn dbColumn = keys[i];
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(dbColumn.ColumnName);
                sb.Append("= ?");
            }

            return sb.ToString();
        }

        public string CreateInsertQuery(string tableName, ICollection<IDbColumn> dbColumns)
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(tableName);
            sb.Append(" (");

            IEnumerator<IDbColumn> enumerator = dbColumns.GetEnumerator();
            int i = 0;

            while (enumerator.MoveNext())
            {
                IDbColumn column = enumerator.Current;
                if (i != 0)
                {
                    sb.Append(",");
                }
                sb.Append(column.ColumnName);
                i++;
            }
            sb.Append(") VALUES (");

            enumerator.Reset();
            i = 0;

            while (enumerator.MoveNext())
            {
                if (i != 0)
                {
                    sb.Append(",");
                }
                sb.Append("?");
                i++;
            } 
            sb.Append(" )");

            return sb.ToString();
        }

        public string CreateUpdateQuery(string tableName, ICollection<IDbColumn> dbColumns)
        {
            var keys = new List<IDbColumn>();
            var values = new List<IDbColumn>();

            foreach (IDbColumn dbColumn in dbColumns)
            {
                if (dbColumn.Key)
                {
                    keys.Add(dbColumn);
                }
                else
                {
                    values.Add(dbColumn);
                }
            }

            var sb = new StringBuilder();
            sb.Append("UPDATE ");
            sb.Append(tableName);
            sb.Append(" SET ");

            for (int i = 0; i < values.Count; i++)
            {
                IDbColumn dbColumn = values[i];
                if (i != 0)
                {
                    sb.Append(",");
                }
                sb.Append(dbColumn.ColumnName);
                sb.Append(" = ?");
            }

            sb.Append(" WHERE ");
            for (int i = 0; i < keys.Count; i++)
            {
                IDbColumn dbColumn = keys[i];
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(dbColumn.ColumnName);
                sb.Append("= ?");
            }

            return sb.ToString();
        }

        public string CreateDeleteQuery(string tableName, ICollection<IDbColumn> dbColumns)
        {
            var keys = new List<IDbColumn>();

            foreach (IDbColumn dbColumn in dbColumns)
            {
                if (dbColumn.Key)
                {
                    keys.Add(dbColumn);
                }
            }

            var sb = new StringBuilder();
            sb.Append("DELETE FROM ");
            sb.Append(tableName);
            sb.Append(" WHERE ");

            for (int i = 0; i < keys.Count; i++)
            {
                IDbColumn dbColumn = keys[i];
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(dbColumn.ColumnName);
                sb.Append("= ?");
            }

            return sb.ToString();
        }

        public string CreateRelatedObjectsLoadQuery(string tableName, IDbRelation relation)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM ");
            sb.Append(CacheManager.TableCache.GetTableName(relation.RelatedObjectType));
            sb.Append(" WHERE ");

            IEnumerator<DbRelationColumnMapping> enumerator = relation.TableColumnMappings.GetEnumerator();
            int i = 0;

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                DbRelationColumnMapping columnMapping = enumerator.Current;
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append( ErDataManagerUtils.FindColumnByAttribute(CacheManager.FieldCache.GetDbColumns(relation.RelatedObjectType),columnMapping.ToField).ColumnName);
                sb.Append("= ?");
                i++;
            }

            return sb.ToString();
        }

        public virtual object ReadFromResultSet(IDataReader reader, IDbColumn dbColumn)
        {
            int ordinal = reader.GetOrdinal(dbColumn.ColumnName);
            if (dbColumn.Nullable)
            {
                object obj = reader.GetValue(ordinal);
                if (obj is System.DBNull)
                {
                    return null;
                }
            }

            switch (dbColumn.ColumnType)
            {
                case DbColumnType.Boolean:
                    return reader.GetBoolean(ordinal);
                case DbColumnType.Char:
                    return reader.GetString(ordinal)[0];
                case DbColumnType.Date:
                    return reader.GetDateTime(ordinal);
                case DbColumnType.Double:
                    return reader.GetDouble(ordinal);
                case DbColumnType.Float:
                    return reader.GetFloat(ordinal);
                case DbColumnType.Integer:
                case DbColumnType.Version:
                    return reader.GetInt32(ordinal);
                case DbColumnType.Long:
                    return reader.GetInt64(ordinal);
                case DbColumnType.Timestamp:
                    return reader.GetDateTime(ordinal);
                case DbColumnType.Varchar:
                    return reader.GetString(ordinal);
                default:
                    return null;
            }
        }

        public void SetToPreparedStatement(IDbCommand cmd, object obj, int parameterIndex, IDbColumn dbColumn)
        {
            IDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = obj;

            switch (dbColumn.ColumnType)
            {
                case DbColumnType.Boolean:
                    parameter.DbType = DbType.Boolean;
                    break;
                case DbColumnType.Char:
                    parameter.DbType = DbType.StringFixedLength;
                    break;
                case DbColumnType.Date:
                    parameter.DbType = DbType.Date;
                    break;
                case DbColumnType.Double:
                    parameter.DbType = DbType.Double;
                    break;
                case DbColumnType.Float:
                    parameter.DbType = DbType.VarNumeric;
                    break;
                case DbColumnType.Integer:
                case DbColumnType.Version:
                    parameter.DbType = DbType.Int32;
                    break;
                case DbColumnType.Long:
                    parameter.DbType = DbType.Int64;
                    break;
                case DbColumnType.Timestamp:
                    parameter.DbType = DbType.DateTime;
                    break;
                case DbColumnType.Varchar:
                    parameter.DbType = DbType.String;
                    break;
            }
        }

        public IDataReader CreateResultSet(IDbConnection con, string sql, DbType[] types, object[] values)
        {
            IDbCommand cmd;

            cmd = con.CreateCommand();
            cmd.CommandText = sql;

            for (int i = 0; i < types.Length; i++)
            {
                DbType type = types[i];
                object value = values[i];

                DbParameter parameter = (OleDbParameter)cmd.CreateParameter();
                parameter.DbType = type;
                parameter.Direction = ParameterDirection.Input;
                parameter.IsNullable = value == null;
                parameter.Value = value;
                cmd.Parameters.Add(parameter);
            }

            return cmd.ExecuteReader();
        }

        #endregion
    }
}