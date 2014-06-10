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
        private AbstractExpressionProcessor _processor;
        
        public AbstractExpressionSelection()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public SelectExpr Expr { get; set; }

        public QuerySelectionExpressionType SelectionType
	 	{
            get { return QuerySelectionExpressionType.Expression; }
		}
			
		public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
		{
            ISegment rootSegment = Expr.RootSegment;
            switch (rootSegment.SegmentType)
            {
                case SegmentType.Group:
                    return _processor.GetGroupFunction((GroupFunctionSegment) rootSegment, true, buildInfo);
                case SegmentType.Field:
                    return _processor.GetFieldName((FieldSegment) rootSegment, true, buildInfo);
                case SegmentType.Query:
                    QuerySegment querySegment = (QuerySegment) rootSegment;
                    buildInfo = dbLayer.DataManipulate().ProcessQuery(buildInfo,querySegment.Query.Structure);
                    return "(" + buildInfo.ExecInfo.Sql + ") as " + querySegment.Alias;
            }
            return null;
		}

        public object Retrieve(IDataReader rs, IDbConnection con, QueryBuildInfo buildInfo)
        {
            try
            {
                string column = null;
                ISegment rootSegment = Expr.RootSegment;

                FieldSegment fieldSegment = null;
                switch (rootSegment.SegmentType)
                {
                    case SegmentType.Group:
                        fieldSegment = ((GroupFunctionSegment)rootSegment).SegmentToGroup;
                        column = GetColumnName(fieldSegment);
                        break;
                    case SegmentType.Field:
                        fieldSegment = (FieldSegment)rootSegment;
                        column = GetColumnName(fieldSegment);
                        break;
                    case SegmentType.Query:
                        QuerySegment querySegment = (QuerySegment)rootSegment;
                        column = querySegment.Alias;
                        break;
                }

                int ordinal = rs.GetOrdinal(column);
                Object obj = rs.GetValue(ordinal);
                return obj;
            }
            catch (Exception ex)
            {
                throw new RetrievalException(ex.Message, ex);
            }

     	}

        private string GetColumnName(FieldSegment fieldSegment)
        {
            string alias = fieldSegment.Alias;
            string column = !string.IsNullOrEmpty(alias) ? alias : _processor.GetColumn(fieldSegment).ColumnName;
            return column;
        }
    }
}
