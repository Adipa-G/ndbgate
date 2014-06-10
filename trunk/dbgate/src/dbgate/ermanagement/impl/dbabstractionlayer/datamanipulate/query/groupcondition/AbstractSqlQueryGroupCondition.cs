using System;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.groupcondition
{
    public class AbstractSqlQueryGroupCondition : IAbstractGroupCondition
    {
        public string Sql { get; set; }

        public QueryGroupConditionExpressionType GroupConditionExpressionType
        {
            get { return QueryGroupConditionExpressionType.RawSql; }
        }

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            return Sql;
        }
    }
}

