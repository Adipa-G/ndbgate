using DbGate.ErManagement.Query;
using System;
using System.Data;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection
{
    public interface IAbstractSelection : IQuerySelection
    {
        String CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo);

        Object Retrieve(IDataReader rs, ITransaction tx, QueryBuildInfo buildInfo);
    }
}