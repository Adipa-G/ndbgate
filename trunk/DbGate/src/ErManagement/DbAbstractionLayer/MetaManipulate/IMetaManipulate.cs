using System.Collections.Generic;
using System.Data;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Support;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate
{
    public interface IMetaManipulate
    {
        void Initialize(IDbConnection con);

        ColumnType MapColumnTypeNameToType(string columnTypeName);

        string MapColumnTypeToTypeName(ColumnType columnTypeId);

        string GetDefaultValueForType(ColumnType columnTypeId);

        ReferentialRuleType MapReferentialRuleNameToType(string ruleTypeName);

        ICollection<IMetaItem> GetMetaData(IDbConnection con);

        ICollection<MetaQueryHolder> CreateDbPathSql(IMetaComparisonGroup metaComparisonGroup);

        bool Equals(IMetaItem iMetaItemA, IMetaItem iMetaItemB);
    }
}