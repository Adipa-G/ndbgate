using System;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.GroupCondition
{
    public interface IAbstractGroupCondition : IQueryGroupCondition
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}