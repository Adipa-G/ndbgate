using DbGate.ErManagement.Query;
using System;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group
{
    public interface IAbstractGroup : IQueryGroup
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}