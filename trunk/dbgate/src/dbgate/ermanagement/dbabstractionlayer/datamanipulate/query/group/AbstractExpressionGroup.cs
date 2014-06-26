using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;
using dbgate.ermanagement.query.expr.segments;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@group
{
    public class AbstractExpressionGroup : IAbstractGroup
	{
        private AbstractExpressionProcessor _processor;
		public GroupExpr Expr { get; set; }

        public AbstractExpressionGroup()
        {
            _processor = new AbstractExpressionProcessor();
        }

        public QueryGroupExpressionType GroupExpressionType
		{
			get {return QueryGroupExpressionType.Expression;}
		}

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
		{
			ISegment rootSegment = Expr.RootSegment;
            switch (rootSegment.SegmentType)
            {
                case SegmentType.Field:
                    return _processor.GetFieldName((FieldSegment) rootSegment, false, buildInfo);
            }
            return null;
		}
	}
}

