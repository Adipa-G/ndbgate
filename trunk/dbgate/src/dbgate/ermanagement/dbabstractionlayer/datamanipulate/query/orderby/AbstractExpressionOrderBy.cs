using System;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;
using dbgate.ermanagement.query.expr.segments;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@orderby
{
    public class AbstractExpressionOrderBy : IAbstractOrderBy
    {
        private AbstractExpressionProcessor _processor;
        public OrderByExpr Expr { get; set; }
        public QueryOrderType OrderType { get; set; }

        public AbstractExpressionOrderBy()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public QueryOrderByExpressionType OrderByExpressionType
		{
			get {return QueryOrderByExpressionType.Expression;}
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
                            sql += " ASC"; break;
                        case QueryOrderType.Descend:
                            sql += " DESC"; break;
                    }
                    return sql;
            }
            return null;
		}
	}
}

