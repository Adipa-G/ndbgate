using DbGate.ErManagement.Query;
using System;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From
{
    public interface IAbstractFrom : IQueryFrom
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);
    }
}