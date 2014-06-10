using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;
using Castle.Core.Internal;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query;
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
    	private IDbLayer _dbLayer;
    	
        public AbstractDataManipulate(IDbLayer dbLayer)
        {
        	_dbLayer = dbLayer;
        	initialize();
		}
	
		protected void initialize()
		{
			QuerySelection.Factory = new AbstractSelectionFactory();
			QueryFrom.Factory = new AbstractFromFactory();
			QueryCondition.Factory = new AbstractConditionFactory();
			QueryGroup.Factory = new AbstractGroupFactory();
			QueryGroupCondition.Factory = new AbstractGroupConditionFactory();
			QueryJoin.Factory = new AbstractJoinFactory();
			QueryOrderBy.Factory = new AbstractOrderByFactory();
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
			SetToPreparedStatement(cmd,obj,parameterIndex,dbColumn.Nullable,dbColumn.ColumnType);
        }

		protected void SetToPreparedStatement (IDbCommand cmd, object obj, int parameterIndex, bool nullable, DbColumnType columnType)
		{
			var parameter = cmd.CreateParameter ();
			parameter.Direction = ParameterDirection.Input;
			parameter.Value = obj;
			if (parameter is OleDbParameter) 
			{
				((OleDbParameter)parameter).IsNullable = true;
			}
			cmd.Parameters.Add(parameter);

            switch (columnType)
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
				SetToPreparedStatement(cmd,param.Value,param.Index,param.Value == null,param.Type);
            }
            return cmd.ExecuteReader();
        }

        public QueryBuildInfo ProcessQuery(QueryBuildInfo buildInfo, QueryStructure structure)
        {
            buildInfo = new QueryBuildInfo(buildInfo);
            buildInfo.CurrentQueryId = structure.QueryId;

            StringBuilder sb = new StringBuilder();
            ProcessFrom(sb, buildInfo, structure);
			ProcessJoin(sb, buildInfo, structure);
            ProcessWhere(sb, buildInfo, structure);
		 	ProcessGroup(sb, buildInfo, structure);
			ProcessGroupCondition(sb, buildInfo, structure);
			ProcessOrderBy(sb, buildInfo, structure);

			AddPagingClause(sb,buildInfo,structure);
            ProcessSelection(sb, buildInfo, structure);

            buildInfo.ExecInfo.Sql = sb.ToString();
            return buildInfo;
        }

		protected void AddPagingClause(StringBuilder sb,QueryBuildInfo buildInfo,QueryStructure structure)
		{
			long pageSize = structure.Fetch;
			long currentOffset = structure.Skip;

			if (currentOffset > 0 && pageSize == 0)
				pageSize = long.MaxValue;

		 	if (pageSize > 0)
		 	{
			 	sb.Append(" LIMIT ? ");
			 	
			 	QueryExecParam param = new QueryExecParam();
			 	param.Index = buildInfo.ExecInfo.Params.Count;
			 	param.Type = DbColumnType.Long;
			 	param.Value = pageSize;
			 	buildInfo.ExecInfo.Params.Add(param);
		 	}
		 	
		 	if (currentOffset > 0)
		 	{
			 	sb.Append(" OFFSET ? ");
			 	
				QueryExecParam param = new QueryExecParam();
				param.Index = buildInfo.ExecInfo.Params.Count;
			 	param.Type = DbColumnType.Long;
			 	param.Value = currentOffset;
			 	buildInfo.ExecInfo.Params.Add(param);
		 	}
	 	}

        private void ProcessSelection(StringBuilder querySb,QueryBuildInfo buildInfo, QueryStructure structure)
        {
            var selectionSb = new StringBuilder();
            selectionSb.Append("SELECT ");
            
             if (structure.SelectList.Count == 0)
                 selectionSb.Append(" * ");

			if (structure.Distinct)
                selectionSb.Append(" DISTINCT ");

            ICollection<IQuerySelection> selections = structure.SelectList;
            bool initial = true;
            foreach (IQuerySelection selection in selections)
            {
                if (!initial)
                {
                    selectionSb.Append(",");
                }
                selectionSb.Append(CreateSelectionSql(selection, buildInfo));
                initial = false;
            }

            querySb.Insert(0, selectionSb.ToString());
        }

        protected string CreateSelectionSql(IQuerySelection selection,QueryBuildInfo buildInfo)
        {
            if (selection != null)
            {
                return ((IAbstractSelection) selection).CreateSql(_dbLayer,buildInfo);
            }
            return "/*Incorrect Selection*/";
        }

        private void ProcessFrom(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
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
                sb.Append(CreateFromSql(from,buildInfo));
                initial = false;
            }
        }

        protected string CreateFromSql(IQueryFrom from,QueryBuildInfo buildInfo)
        {
            if (from != null)
            {
                return ((IAbstractFrom) from).CreateSql(_dbLayer,buildInfo);
            }
            return "/*Incorrect From*/";
        }

		private void ProcessJoin(StringBuilder sb,QueryBuildInfo buildInfo, QueryStructure structure)
        {
			var joinList = structure.JoinList;
			if (joinList.Count == 0)
				return;

			foreach (var join in joinList) 
			{
				sb.Append(" ");
				sb.Append(CreateJoinSql(join,buildInfo));
			}
        }

        protected string CreateJoinSql(IQueryJoin join,QueryBuildInfo buildInfo)
        {
            if (join != null)
            {
                return ((IAbstractJoin) join).CreateSql(_dbLayer,buildInfo);
            }
            return "/*Incorrect Join*/";
        }

		private void ProcessWhere(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
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
			 	sb.Append(CreateWhereSql(condition,buildInfo));
			 	initial = false;
		 	}
	 	}
		 	
	 	protected string CreateWhereSql(IQueryCondition condition,QueryBuildInfo buildInfo)
	 	{
			if (condition != null)
		 	{
		 		return ((IAbstractCondition) condition).CreateSql(_dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Where*/";
	 	}
		 	
	 	private void ProcessGroup(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
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
			 	sb.Append(CreateGroupSql(group,buildInfo));
			 	initial = false;
		 	}
	 	}
	 	
	 	protected string CreateGroupSql(IQueryGroup group,QueryBuildInfo buildInfo)
	 	{
			if (group != null)
		 	{
		 		return ((IAbstractGroup) group).CreateSql(_dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Group*/";
	 	}

		private void ProcessGroupCondition(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
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
			 	sb.Append(CreateGroupConditionSql(groupCondition,buildInfo));
			 	initial = false;
		 	}
	 	}

        protected string CreateGroupConditionSql(IQueryGroupCondition groupCondition, QueryBuildInfo buildInfo)
	 	{
			if (groupCondition != null)
		 	{
		 		return ((IAbstractGroupCondition) groupCondition).CreateSql(_dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Group condition*/";
	 	}

		private void ProcessOrderBy(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
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
			 	sb.Append(CreateOrderBySql(orderBy,buildInfo));
			 	initial = false;
		 	}
	 	}
		 	
	 	protected string CreateOrderBySql(IQueryOrderBy orderBy, QueryBuildInfo buildInfo)
	 	{
			if (orderBy != null)
		 	{
		 		return ((IAbstractOrderBy) orderBy).CreateSql(_dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Order by*/";
	 	}
        #endregion
    }
}