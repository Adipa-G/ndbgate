using System;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From
{
    public interface IAbstractFrom : IQueryFrom
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}