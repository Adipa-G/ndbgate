using System.Collections.Generic;
using System.Data;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate
{
    public interface IDataManipulate
    {
        string CreateLoadQuery(string tableName, ICollection<IColumn> dbColumns);

        string CreateInsertQuery(string tableName, ICollection<IColumn> dbColumns);

        string CreateUpdateQuery(string tableName, ICollection<IColumn> dbColumns);

        string CreateDeleteQuery(string tableName, ICollection<IColumn> dbColumns);

        string CreateRelatedObjectsLoadQuery(IRelation relation);

        object ReadFromResultSet(IDataReader reader, IColumn column);

        void SetToPreparedStatement(IDbCommand cmd, object obj, int parameterIndex, IColumn column);

        IDataReader CreateResultSet(ITransaction tx, QueryExecInfo execInfo);

        QueryBuildInfo ProcessQuery(QueryBuildInfo buildInfo, QueryStructure structure);
    }
}