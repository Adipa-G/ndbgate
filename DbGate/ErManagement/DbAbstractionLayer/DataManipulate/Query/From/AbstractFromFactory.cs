using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From
{
    public class AbstractFromFactory
    {
        public IAbstractFrom CreateFrom(QueryFromExpressionType expressionType)
        {
            switch (expressionType)
            {
                case QueryFromExpressionType.RawSql:
                    return new AbstractSqlQueryFrom();
                case QueryFromExpressionType.EntityType:
                    return new AbstractTypeFrom();
                case QueryFromExpressionType.Query:
                    return new AbstractSubQueryFrom();
                case QueryFromExpressionType.QueryUnion:
                    return new AbstractUnionFrom();
                default:
                    return null;
            }
        }
    }
}