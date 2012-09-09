using System.Collections.Generic;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate
{
    public interface IDataManipulate
    {
        string CreateLoadQuery(string tableName, ICollection<IDbColumn> dbColumns);

        string CreateInsertQuery(string tableName, ICollection<IDbColumn> dbColumns);

        string CreateUpdateQuery(string tableName, ICollection<IDbColumn> dbColumns);

        string CreateDeleteQuery(string tableName, ICollection<IDbColumn> dbColumns);

        string CreateRelatedObjectsLoadQuery(string tableName, IDbRelation relation);

        object ReadFromResultSet(IDataReader reader, IDbColumn dbColumn);

        void SetToPreparedStatement(IDbCommand cmd, object obj, int parameterIndex, IDbColumn dbColumn);

        IDataReader CreateResultSet(IDbConnection con, QueryExecInfo execInfo);

        QueryExecInfo CreateExecInfo(IDbConnection con, ISelectionQuery query);
    }
}