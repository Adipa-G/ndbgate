using DbGate.ErManagement.Query;
using System;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.OrderBy
{
    public interface IAbstractOrderBy : IQueryOrderBy
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}