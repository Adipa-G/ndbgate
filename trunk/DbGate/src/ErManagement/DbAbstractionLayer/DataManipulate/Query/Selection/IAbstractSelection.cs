using System;
using System.Data;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection
{
    public interface IAbstractSelection : IQuerySelection
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);

        Object Retrieve(IDataReader rs, IDbConnection con, QueryBuildInfo buildInfo);
    }
}