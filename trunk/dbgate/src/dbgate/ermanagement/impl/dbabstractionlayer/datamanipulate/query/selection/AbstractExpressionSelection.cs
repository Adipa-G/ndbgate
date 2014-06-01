using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.query.expr;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr.segments;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
    public class AbstractExpressionSelection : IAbstractSelection
    {
        private Type _entityType;
		private string _field;
		private string _alias;
		private string _function;

        public SelectExpr Expr { get; set; }
			
        public QuerySelectionExpressionType SelectionType
	 	{
            get { return QuerySelectionExpressionType.Expression; }
		}
		
		private void ProcessExpr()
		{
		    if (_entityType != null)
		        return;
		
		    ISegment rootSegment = Expr.RootSegment;
		    GroupFunctionSegment groupSegment = null;
		    FieldSegment fieldSegment = null;
		
		    if (rootSegment.SegmentType == SegmentType.Group)
		    {
		        groupSegment = (GroupFunctionSegment) rootSegment;
		        fieldSegment = (FieldSegment) groupSegment.SegmentToGroup;
		    }
		    if (rootSegment.SegmentType == SegmentType.Field)
		    {
		        fieldSegment = (FieldSegment) rootSegment;
		    }
		
		    if (fieldSegment != null)
		    {
		        _entityType = fieldSegment.EntityType;
		        _field = fieldSegment.Field;
		        _alias = fieldSegment.Alias;
		    }
		
		    if (groupSegment != null)
		    {
		        switch (groupSegment.GroupFunctionType)
		        {
		            case GroupFunctionSegmentType.Count:
		                _function = "COUNT";
		                break;
		            case GroupFunctionSegmentType.Sum:
		                _function = "SUM";
		                break;
		            case GroupFunctionSegmentType.CustFunc:
		                _function = groupSegment.CustFunction;
		                break;
		         }
		     }
		}
		
		private IDbColumn GetColumn(QueryBuildInfo buildInfo)
		{
		    ICollection<IDbColumn> columns = null;
		    try
		    {
		        columns = CacheManager.FieldCache.GetDbColumns(_entityType);
		    }
			catch (FieldCacheMissException)
			{
			    try
			    {
			        CacheManager.FieldCache.Register(_entityType,(IServerRoDbClass)Activator.CreateInstance(_entityType));
			        columns = CacheManager.FieldCache.GetDbColumns(_entityType);
			    }
			    catch (Exception ex)
			    {
                    Console.Write(ex.StackTrace);
			    }
			}
			
			foreach (IDbColumn column in columns)
			{
			    if (column.AttributeName.Equals(_field))
			    {
			        return column;
			    }
			}
			return null;
		}
			
			
		public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
		{
		    ProcessExpr();
		    string tableAlias = buildInfo.GetAlias(_entityType);
		    tableAlias = (tableAlias == null)?"" : tableAlias + ".";
		    IDbColumn column = GetColumn(buildInfo);
			
		    if (column != null)
		    {
		        string sql = "";
		        if (!string.IsNullOrEmpty(_function))
		        {
		            sql = _function + "(" +tableAlias + column.ColumnName + ")";
		        }
		        else
		        {
		            sql = tableAlias + column.ColumnName;
		        }
		        if (!string.IsNullOrEmpty(_alias))
		        {
		            sql = sql + " AS " + _alias;
		        }
		        return sql;
		    }
		    else
		    {
		        return "<incorrect column for " + _field + ">";
		    }
		}

        public object Retrieve(IDataReader rs, IDbConnection con, QueryBuildInfo buildInfo)
        {
		    try
		    {
		        ProcessExpr();
		        string columnName = !string.IsNullOrEmpty(_alias)? _alias : GetColumn(buildInfo).ColumnName;
		        int ordinal = rs.GetOrdinal(columnName);
		        object obj = rs.GetValue(ordinal);
		        return obj;
		    }
		    catch (Exception ex)
		    {
		        throw new RetrievalException(ex.Message,ex);
		    }
     	}
    }
}
