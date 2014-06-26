using System.Collections.Generic;
using System.Data;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate.compare;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate.datastructures;
using dbgate.ermanagement.dbabstractionlayer.metamanipulate.support;

namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate
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