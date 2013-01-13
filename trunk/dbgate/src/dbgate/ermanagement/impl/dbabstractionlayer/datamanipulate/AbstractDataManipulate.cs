﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;
using Castle.Core.Internal;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.impl.utils;
using System.Linq;
using dbgate.ermanagement.query;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.from;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.condition;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.group;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.join;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.orderby;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate
{
    public class AbstractDataManipulate : IDataManipulate
    {
        public AbstractDataManipulate(IDbLayer dbLayer)
        {
        	initialize();
		}
	
		protected void initialize()
		{
			QuerySelection.Factory = new AbstractQuerySelectionFactory();
			QueryFrom.Factory = new AbstractQueryFromFactory();
			QueryCondition.Factory = new AbstractQueryConditionFactory();
			QueryGroup.Factory = new AbstractQueryGroupFactory();
			QueryGroupCondition.Factory = new AbstractQueryGroupConditionFactory();
			QueryJoin.Factory = new AbstractQueryJoinFactory();
			QueryOrderBy.Factory = new AbstractQueryOrderByFactory();
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

        public IDataReader CreateResultSet(IDbConnection con, QueryExecInfo execInfo)
        {
            IDbCommand cmd;

            cmd = con.CreateCommand();
            cmd.CommandText = execInfo.Sql;

            var cmdParams = execInfo.Params;
            var sortedParams = cmdParams.OrderBy(p => p.Index);

            foreach (QueryExecParam param in sortedParams)
            {
                DbParameter parameter = (OleDbParameter)cmd.CreateParameter();
                parameter.DbType = param.Type;
                parameter.Direction = ParameterDirection.Input;
                parameter.IsNullable = param.Value == null;
                parameter.Value = param.Value;
                cmd.Parameters.Add(parameter);  
            }

            return cmd.ExecuteReader();
        }

        public QueryExecInfo CreateExecInfo(IDbConnection con, ISelectionQuery query)
        {
            QueryStructure structure = query.Structure;
            return ProcessQuery(null, structure);
        }

        protected QueryExecInfo ProcessQuery(QueryExecInfo execInfo, QueryStructure structure)
        {
            if (execInfo == null)
            {
                execInfo = new QueryExecInfo();
            }

            StringBuilder sb = new StringBuilder();
            ProcessSelection(sb, execInfo, structure);
            ProcessFrom(sb, execInfo, structure);
			ProcessJoin(sb, execInfo, structure);
            ProcessWhere(sb, execInfo, structure);
		 	ProcessGroup(sb, execInfo, structure);
			ProcessGroupCondition(sb, execInfo, structure);
			ProcessOrderBy(sb, execInfo, structure);

            execInfo.Sql = sb.ToString();
            return execInfo;
        }

        private void ProcessSelection(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
        {
            sb.Append("SELECT ");

            ICollection<IQuerySelection> selections = structure.SelectList;
            bool initial = true;
            foreach (IQuerySelection selection in selections)
            {
                if (!initial)
                {
                    sb.Append(",");
                }
                sb.Append(CreateSelectionSql(selection));
                initial = false;
            }
        }

        protected String CreateSelectionSql(IQuerySelection selection)
        {
            if (selection != null)
            {
                return ((IAbstractQuerySelection) selection).CreateSql();
            }
            return "/*Incorrect Selection*/";
        }

        private void ProcessFrom(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
        {
            sb.Append(" FROM ");

            ICollection<IQueryFrom> fromList = structure.FromList;
            bool initial = true;
            foreach (IQueryFrom from in fromList)
            {
                if (!initial)
                {
                    sb.Append(",");
                }
                sb.Append(CreateFromSql(from));
                initial = false;
            }
        }

        protected String CreateFromSql(IQueryFrom from)
        {
            if (from != null)
            {
                return ((IAbstractQueryFrom) from).CreateSql();
            }
            return "/*Incorrect From*/";
        }

		private void ProcessJoin(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
        {
			var joinList = structure.JoinList;
			if (joinList.Count == 0)
				return;

			foreach (var join in joinList) 
			{
				sb.Append(" ");
				sb.Append(CreateJoinSql(join));
			}
        }

        protected String CreateJoinSql(IQueryJoin join)
        {
            if (join != null)
            {
                return ((IAbstractQueryJoin) join).CreateSql();
            }
            return "/*Incorrect Join*/";
        }

		private void ProcessWhere(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
	 	{
		 	ICollection<IQueryCondition> conditionList = structure.ConditionList;
		 	if (conditionList.Count == 0)
		 	return;
		 	
		 	sb.Append(" WHERE ");
		 	
		 	bool initial = true;
			foreach (IQueryCondition condition in conditionList)
		 	{
			 	if (!initial)
			 	{
			 		sb.Append(" AND ");
			 	}
			 	sb.Append(CreateWhereSql(condition));
			 	initial = false;
		 	}
	 	}
		 	
	 	protected String CreateWhereSql(IQueryCondition condition)
	 	{
			if (condition != null)
		 	{
		 		return ((IAbstractQueryCondition) condition).CreateSql();
		 	}
		 	return "/*Incorrect Where*/";
	 	}
		 	
	 	private void ProcessGroup(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
	 	{
		 	ICollection<IQueryGroup> groupList = structure.GroupList;
		 	if (groupList.Count == 0)
		 	return;
		 	
		 	sb.Append(" GROUP BY ");
		 	
		 	bool initial = true;
		 	foreach (IQueryGroup group in groupList)
			 	{
			 	if (!initial)
			 	{
			 		sb.Append(",");
			 	}
			 	sb.Append(CreateGroupSql(group));
			 	initial = false;
		 	}
	 	}
	 	
	 	protected String CreateGroupSql(IQueryGroup group)
	 	{
			if (group != null)
		 	{
		 		return ((IAbstractQueryGroup) group).CreateSql();
		 	}
		 	return "/*Incorrect Group*/";
	 	}

		private void ProcessGroupCondition(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
	 	{
		 	ICollection<IQueryGroupCondition> groupConditionList = structure.GroupConditionList;
		 	if (groupConditionList.Count == 0)
		 	return;
		 	
		 	sb.Append(" HAVING ");
		 	
		 	bool initial = true;
			foreach (IQueryGroupCondition groupCondition in groupConditionList)
		 	{
			 	if (!initial)
			 	{
			 		sb.Append(" AND ");
			 	}
			 	sb.Append(CreateGroupConditionSql(groupCondition));
			 	initial = false;
		 	}
	 	}
		 	
	 	protected String CreateGroupConditionSql(IQueryGroupCondition groupCondition)
	 	{
			if (groupCondition != null)
		 	{
		 		return ((IAbstractQueryGroupCondition) groupCondition).CreateSql();
		 	}
		 	return "/*Incorrect Group condition*/";
	 	}

		private void ProcessOrderBy(StringBuilder sb, QueryExecInfo execInfo, QueryStructure structure)
	 	{
		 	ICollection<IQueryOrderBy> orderList = structure.OrderList;
		 	if (orderList.Count == 0)
		 	return;
		 	
		 	sb.Append(" ORDER BY ");
		 	
		 	bool initial = true;
			foreach (IQueryOrderBy orderBy in orderList)
		 	{
			 	if (!initial)
			 	{
			 		sb.Append(",");
			 	}
			 	sb.Append(CreateOrderBySql(orderBy));
			 	initial = false;
		 	}
	 	}
		 	
	 	protected String CreateOrderBySql(IQueryOrderBy orderBy)
	 	{
			if (orderBy != null)
		 	{
		 		return ((IAbstractQueryOrderBy) orderBy).CreateSql();
		 	}
		 	return "/*Incorrect Order by*/";
	 	}
        #endregion
    }
}