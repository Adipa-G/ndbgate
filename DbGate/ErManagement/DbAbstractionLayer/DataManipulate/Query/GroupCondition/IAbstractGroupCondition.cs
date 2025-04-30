using DbGate.ErManagement.Query;
using System;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition
{
    public interface IAbstractGroupCondition : IQueryGroupCondition
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}