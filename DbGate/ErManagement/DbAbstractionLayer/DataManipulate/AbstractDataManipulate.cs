﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DbGate.Caches;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Condition;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.OrderBy;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.ErManagement.Query;
using System.Linq;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate
{
    public abstract class AbstractDataManipulate : IDataManipulate
    {
    	private IDbLayer dbLayer;
    	
        protected AbstractDataManipulate(IDbLayer dbLayer)
        {
        	this.dbLayer = dbLayer;
        	Initialize();
		}
	
		protected void Initialize()
		{
			QuerySelection.Factory = new AbstractSelectionFactory();
			QueryFrom.Factory = new AbstractFromFactory();
			QueryCondition.Factory = new AbstractConditionFactory();
			QueryGroup.Factory = new AbstractGroupFactory();
			QueryGroupCondition.Factory = new AbstractGroupConditionFactory();
			QueryJoin.Factory = new AbstractJoinFactory();
			QueryOrderBy.Factory = new AbstractOrderByFactory();
	 	}

        protected virtual string FixUpQuery(string query)
        {
            return query;
        }

        #region IDataManipulate Members

        public string CreateLoadQuery(string tableName, ICollection<IColumn> dbColumns)
        {
            var keys = new List<IColumn>();

            foreach (var dbColumn in dbColumns)
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

            for (var i = 0; i < keys.Count; i++)
            {
                var column = keys[i];
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(column.ColumnName);
                sb.Append("= ?");
            }

            return FixUpQuery(sb.ToString());
        }

        public string CreateInsertQuery(string tableName, ICollection<IColumn> dbColumns)
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(tableName);
            sb.Append(" (");

            var enumerator = dbColumns.GetEnumerator();
            var i = 0;

            while (enumerator.MoveNext())
            {
                var column = enumerator.Current;
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

            return FixUpQuery(sb.ToString());
        }

        public string CreateUpdateQuery(string tableName, ICollection<IColumn> dbColumns)
        {
            var keys = new List<IColumn>();
            var values = new List<IColumn>();

            foreach (var dbColumn in dbColumns)
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

            for (var i = 0; i < values.Count; i++)
            {
                var column = values[i];
                if (i != 0)
                {
                    sb.Append(",");
                }
                sb.Append(column.ColumnName);
                sb.Append(" = ?");
            }

            sb.Append(" WHERE ");
            for (var i = 0; i < keys.Count; i++)
            {
                var column = keys[i];
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(column.ColumnName);
                sb.Append("= ?");
            }

            return FixUpQuery(sb.ToString());
        }

        public string CreateDeleteQuery(string tableName, ICollection<IColumn> dbColumns)
        {
            var keys = new List<IColumn>();

            foreach (var dbColumn in dbColumns)
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

            for (var i = 0; i < keys.Count; i++)
            {
                var column = keys[i];
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append(column.ColumnName);
                sb.Append("= ?");
            }

            return FixUpQuery(sb.ToString());
        }

        public string CreateRelatedObjectsLoadQuery(IRelation relation)
        {
            var entityInfo = CacheManager.GetEntityInfo(relation.RelatedObjectType);

            var sb = new StringBuilder();
            sb.Append("SELECT * FROM ");
            sb.Append(entityInfo.TableInfo.TableName);
            sb.Append(" WHERE ");

            var enumerator = relation.TableColumnMappings.GetEnumerator();
            var i = 0;

            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                var columnMapping = enumerator.Current;
                if (i != 0)
                {
                    sb.Append(" AND ");
                }
                sb.Append( entityInfo.FindColumnByAttribute(columnMapping.ToField).ColumnName);
                sb.Append("= ?");
                i++;
            }

            return FixUpQuery(sb.ToString());
        }

        public virtual object ReadFromResultSet(IDataReader reader, IColumn column)
        {
            var ordinal = reader.GetOrdinal(column.ColumnName);
            if (column.Nullable)
            {
                var obj = reader.GetValue(ordinal);
                if (obj is System.DBNull)
                {
                    return null;
                }
            }

            switch (column.ColumnType)
            {
                case ColumnType.Boolean:
                    return reader.GetBoolean(ordinal);
                case ColumnType.Char:
                    return reader.GetString(ordinal)[0];
                case ColumnType.Date:
                    return reader.GetDateTime(ordinal);
                case ColumnType.Double:
                    return reader.GetDouble(ordinal);
                case ColumnType.Float:
                    return reader.GetFloat(ordinal);
                case ColumnType.Integer:
                case ColumnType.Version:
                    return reader.GetInt32(ordinal);
                case ColumnType.Long:
                    return reader.GetInt64(ordinal);
                case ColumnType.Timestamp:
                    return reader.GetDateTime(ordinal);
                case ColumnType.Varchar:
                    return reader.GetString(ordinal);
                case ColumnType.Guid:
                    return reader.GetGuid(ordinal);
                default:
                    return null;
            }
        }

        public void SetToPreparedStatement(IDbCommand cmd, object obj, int parameterIndex, IColumn column)
        {
			SetToPreparedStatement(cmd,obj,parameterIndex,column.Nullable,column.ColumnType);
        }

		protected virtual void SetToPreparedStatement (IDbCommand cmd, object obj, int parameterIndex, bool nullable, ColumnType columnType)
		{
			var parameter = cmd.CreateParameter ();
			parameter.Direction = ParameterDirection.Input;
			parameter.Value = obj ?? DBNull.Value;
            
			cmd.Parameters.Add(parameter);
		    parameter.DbType = ColumnTypeMapping.GetSqlType(columnType);
        }

        public IDataReader CreateResultSet(ITransaction tx, QueryExecInfo execInfo)
        {
            IDbCommand cmd;

            cmd = tx.CreateCommand();
            cmd.CommandText = execInfo.Sql;

            var cmdParams = execInfo.Params;
            var sortedParams = cmdParams.OrderBy(p => p.Index);

            foreach (var param in sortedParams)
            {
				SetToPreparedStatement(cmd,param.Value,param.Index + 1,param.Value == null,param.Type);
            }
            return cmd.ExecuteReader();
        }

        public QueryBuildInfo ProcessQuery(QueryBuildInfo buildInfo, QueryStructure structure)
        {
            buildInfo = new QueryBuildInfo(buildInfo);
            buildInfo.CurrentQueryId = structure.QueryId;

            var sb = new StringBuilder();
            ProcessFrom(sb, buildInfo, structure);
			ProcessJoin(sb, buildInfo, structure);
            ProcessWhere(sb, buildInfo, structure);
		 	ProcessGroup(sb, buildInfo, structure);
			ProcessGroupCondition(sb, buildInfo, structure);
			ProcessOrderBy(sb, buildInfo, structure);

			AddPagingClause(sb,buildInfo,structure);
            ProcessSelection(sb, buildInfo, structure);

            buildInfo.ExecInfo.Sql = FixUpQuery(sb.ToString());
            return buildInfo;
        }

		protected void AddPagingClause(StringBuilder sb,QueryBuildInfo buildInfo,QueryStructure structure)
		{
			var pageSize = structure.Fetch;
			var currentOffset = structure.Skip;

			if (currentOffset > 0 && pageSize == 0)
				pageSize = long.MaxValue;

		 	if (pageSize > 0)
		 	{
			 	sb.Append(" LIMIT ? ");
			 	
			 	var param = new QueryExecParam();
			 	param.Index = buildInfo.ExecInfo.Params.Count;
			 	param.Type = ColumnType.Long;
			 	param.Value = pageSize;
			 	buildInfo.ExecInfo.Params.Add(param);
		 	}
		 	
		 	if (currentOffset > 0)
		 	{
			 	sb.Append(" OFFSET ? ");
			 	
				var param = new QueryExecParam();
				param.Index = buildInfo.ExecInfo.Params.Count;
			 	param.Type = ColumnType.Long;
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

            var selections = structure.SelectList;
            var initial = true;
            foreach (var selection in selections)
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
                return ((IAbstractSelection) selection).CreateSql(dbLayer,buildInfo);
            }
            return "/*Incorrect Selection*/";
        }

        private void ProcessFrom(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
        {
            sb.Append(" FROM ");

            var fromList = structure.FromList;
            var initial = true;
            foreach (var from in fromList)
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
                return ((IAbstractFrom) from).CreateSql(dbLayer,buildInfo);
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
                return ((IAbstractJoin) join).CreateSql(dbLayer,buildInfo);
            }
            return "/*Incorrect Join*/";
        }

		private void ProcessWhere(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
	 	{
		 	var conditionList = structure.ConditionList;
		 	if (conditionList.Count == 0)
		 	return;
		 	
		 	sb.Append(" WHERE ");
		 	
		 	var initial = true;
			foreach (var condition in conditionList)
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
		 		return ((IAbstractCondition) condition).CreateSql(dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Where*/";
	 	}
		 	
	 	private void ProcessGroup(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
	 	{
		 	var groupList = structure.GroupList;
		 	if (groupList.Count == 0)
		 	return;
		 	
		 	sb.Append(" GROUP BY ");
		 	
		 	var initial = true;
		 	foreach (var group in groupList)
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
		 		return ((IAbstractGroup) group).CreateSql(dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Group*/";
	 	}

		private void ProcessGroupCondition(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
	 	{
		 	var groupConditionList = structure.GroupConditionList;
		 	if (groupConditionList.Count == 0)
		 	return;
		 	
		 	sb.Append(" HAVING ");
		 	
		 	var initial = true;
			foreach (var groupCondition in groupConditionList)
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
		 		return ((IAbstractGroupCondition) groupCondition).CreateSql(dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Group condition*/";
	 	}

		private void ProcessOrderBy(StringBuilder sb, QueryBuildInfo buildInfo, QueryStructure structure)
	 	{
		 	var orderList = structure.OrderList;
		 	if (orderList.Count == 0)
		 	return;
		 	
		 	sb.Append(" ORDER BY ");
		 	
		 	var initial = true;
			foreach (var orderBy in orderList)
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
		 		return ((IAbstractOrderBy) orderBy).CreateSql(dbLayer,buildInfo);
		 	}
		 	return "/*Incorrect Order by*/";
	 	}
        #endregion
    }
}