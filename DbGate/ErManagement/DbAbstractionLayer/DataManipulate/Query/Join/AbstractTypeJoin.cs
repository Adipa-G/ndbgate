using System;
using System.Collections.Generic;
using System.Text;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join
{
    public class AbstractTypeJoin : IAbstractJoin
    {
        private readonly AbstractExpressionProcessor processor;

        public AbstractTypeJoin()
        {
            JoinType = QueryJoinType.Inner;
            processor = new AbstractExpressionProcessor();
        }

        public Type TypeFrom { get; set; }
        public Type TypeTo { get; set; }
        public JoinExpr Expr { get; set; }
        public string TypeToAlias { get; set; }
        public QueryJoinType JoinType { get; set; }

        #region IAbstractJoin Members

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            if (Expr == null)
            {
                CreateJoinExpressionForDefinedRelation(buildInfo);
            }
            return CreateSqlForExpression(dbLayer, buildInfo);
        }

        public QueryJoinExpressionType JoinExpressionType => QueryJoinExpressionType.Type;

        #endregion

        private void CreateJoinExpressionForDefinedRelation(QueryBuildInfo buildInfo)
        {
            var typeFromAlias = buildInfo.GetAlias(TypeFrom);
            var relation = processor.GetRelation(TypeFrom, TypeTo);
            if (relation == null)
            {
                relation = processor.GetRelation(TypeTo, TypeFrom);
            }

            if (relation != null)
            {
                Expr = JoinExpr.Build();
                var tableColumnMappings = relation.TableColumnMappings;
                var i = 0;
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

        private String CreateSqlForExpression(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            var from = new AbstractTypeFrom();
            from.EntityType = TypeTo;
            from.Alias = TypeToAlias;

            var sb = new StringBuilder();
            AppendJoinTypeSql(sb);
            sb.Append(from.CreateSql(dbLayer, buildInfo));
            sb.Append(" ON ");
            processor.Process(sb, Expr.RootSegment, buildInfo, dbLayer);
            return sb.ToString();
        }

        private void AppendJoinTypeSql(StringBuilder sb)
        {
            switch (JoinType)
            {
                case QueryJoinType.Inner:
                    sb.Append("INNER JOIN ");
                    break;
                case QueryJoinType.Left:
                    sb.Append("LEFT JOIN ");
                    break;
                case QueryJoinType.Right:
                    sb.Append("RIGHT JOIN ");
                    break;
                case QueryJoinType.Full:
                    sb.Append("FULL JOIN ");
                    break;
            }
        }
    }
}