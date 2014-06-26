using System;
using System.Text;
using dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@from;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;

namespace dbgate.ermanagement.dbabstractionlayer.datamanipulate.query.@join
{
	public class AbstractTypeJoin : IAbstractJoin
	{
		private readonly AbstractExpressionProcessor processor;

	    public Type TypeFrom { get; set; }
	    public Type TypeTo { get; set; }
	    public JoinExpr Expr { get; set; }
	    public string TypeToAlias { get; set; }
	    public QueryJoinType JoinType { get; set; }
    
        public AbstractTypeJoin()
        {
            JoinType = QueryJoinType.Inner;
            processor = new AbstractExpressionProcessor();
        }
    
        public string CreateSql(IDbLayer dbLayer,QueryBuildInfo buildInfo)
        {
            if (Expr == null)
            {
                CreateJoinExpressionForDefinedRelation(buildInfo);
            }
            return CreateSqlForExpression(dbLayer,buildInfo);
        }
    
        private void CreateJoinExpressionForDefinedRelation(QueryBuildInfo buildInfo)
        {
            string typeFromAlias = buildInfo.GetAlias(TypeFrom);
            IRelation relation = processor.GetRelation(TypeFrom,TypeTo);
            if (relation == null)
            {
                relation = processor.GetRelation(TypeTo,TypeFrom);
            }
    
            if (relation != null)
            {
                Expr = JoinExpr.Build();
                var tableColumnMappings = relation.TableColumnMappings;
                int i = 0;
                foreach (var mapping in tableColumnMappings)
                {
                    if (i > 0)
                    {
                        Expr.And();
                    }
                    Expr.Field(TypeFrom, typeFromAlias, mapping.FromField);
                    Expr.Eq();
                    Expr.Field(TypeTo, TypeToAlias, mapping.ToField);
                    i++;
                }
            }
        }
    
        private String CreateSqlForExpression(IDbLayer dbLayer,QueryBuildInfo buildInfo)
        {
            var from = new AbstractTypeFrom();
            from.EntityType = TypeTo;
            from.Alias = TypeToAlias;
    
            var sb = new StringBuilder();
            AppendJoinTypeSql(sb);
            sb.Append(from.CreateSql(dbLayer,buildInfo));
            sb.Append(" ON ");
            processor.Process(sb,Expr.RootSegment,buildInfo,dbLayer);
            return sb.ToString();
        }
    
        private void AppendJoinTypeSql(StringBuilder sb)
        {
            switch (JoinType)
            {
                case QueryJoinType.Inner:
                    sb.Append("INNER JOIN "); break;
                case QueryJoinType.Left:
                    sb.Append("LEFT JOIN "); break;
                case QueryJoinType.Right:
                    sb.Append("RIGHT JOIN "); break;
                case QueryJoinType.Full:
                    sb.Append("FULL JOIN ");break;
            }
        }

	    public QueryJoinExpressionType JoinExpressionType
	    {
            get { return QueryJoinExpressionType.Type; }
	    }
	}
}

