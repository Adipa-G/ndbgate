using System;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.ErManagement.Query.Expr.Segments;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.OrderBy
{
    public class AbstractExpressionOrderBy : IAbstractOrderBy
    {
        private readonly AbstractExpressionProcessor _processor;

        public AbstractExpressionOrderBy()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public OrderByExpr Expr { get; set; }
        public QueryOrderType OrderType { get; set; }

        #region IAbstractOrderBy Members

        public QueryOrderByExpressionType OrderByExpressionType
        {
            get { return QueryOrderByExpressionType.Expression; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            ISegment rootSegment = Expr.RootSegment;
            switch (rootSegment.SegmentType)
            {
                case SegmentType.Field:
                    String sql = _processor.GetFieldName((FieldSegment) rootSegment, false, buildInfo);
                    switch (OrderType)
                    {
                        case QueryOrderType.Ascend:
                            sql += " ASC";
                            break;
                        case QueryOrderType.Descend:
                            sql += " DESC";
                            break;
                    }
                    return sql;
            }
            return null;
        }

        #endregion
    }
}