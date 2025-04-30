using DbGate.ErManagement.Query;
using System;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Join
{
    public interface IAbstractJoin : IQueryJoin
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}