using System;
using System.Data;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.ErManagement.Query.Expr.Segments;
using DbGate.Exceptions;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection
{
    public class AbstractExpressionSelection : IAbstractSelection
    {
        private readonly AbstractExpressionProcessor processor;

        public AbstractExpressionSelection()
        {
            processor = new AbstractExpressionProcessor();
        }

        public SelectExpr Expr { get; set; }

        #region IAbstractSelection Members

        public QuerySelectionExpressionType SelectionType => QuerySelectionExpressionType.Expression;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            var rootSegment = Expr.RootSegment;
            switch (rootSegment.SegmentType)
            {
                case SegmentType.Group:
                    return processor.GetGroupFunction((GroupFunctionSegment) rootSegment, true, buildInfo);
                case SegmentType.Field:
                    return processor.GetFieldName((FieldSegment) rootSegment, true, buildInfo);
                case SegmentType.Query:
                    var querySegment = (QuerySegment) rootSegment;
                    buildInfo = dbLayer.DataManipulate().ProcessQuery(buildInfo, querySegment.Query.Structure);
                    return "(" + buildInfo.ExecInfo.Sql + ") as " + querySegment.Alias;
            }
            return null;
        }

        public object Retrieve(IDataReader rs, ITransaction tx, QueryBuildInfo buildInfo)
        {
            try
            {
                string column = null;
                var rootSegment = Expr.RootSegment;

                FieldSegment fieldSegment = null;
                switch (rootSegment.SegmentType)
                {
                    case SegmentType.Group:
                        fieldSegment = ((GroupFunctionSegment) rootSegment).SegmentToGroup;
                        column = GetColumnName(fieldSegment,buildInfo);
                        break;
                    case SegmentType.Field:
                        fieldSegment = (FieldSegment) rootSegment;
                        column = GetColumnName(fieldSegment,buildInfo);
                        break;
                    case SegmentType.Query:
                        var querySegment = (QuerySegment) rootSegment;
                        column = querySegment.Alias;
                        break;
                }

                var ordinal = rs.GetOrdinal(column);
                var obj = rs.GetValue(ordinal);
                return obj;
            }
            catch (Exception ex)
            {
                throw new RetrievalException(ex.Message, ex);
            }
        }

        #endregion

        private string GetColumnName(FieldSegment fieldSegment,QueryBuildInfo buildInfo)
        {
            var alias = fieldSegment.Alias;
            var column = !string.IsNullOrEmpty(alias) ? alias : processor.GetColumn(fieldSegment,buildInfo).ColumnName;
            return column;
        }
    }
}