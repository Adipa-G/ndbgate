using System;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Group
{
    public interface IAbstractGroup : IQueryGroup
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}