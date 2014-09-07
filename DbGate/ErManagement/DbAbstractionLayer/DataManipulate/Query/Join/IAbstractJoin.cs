using System;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join
{
    public interface IAbstractJoin : IQueryJoin
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}