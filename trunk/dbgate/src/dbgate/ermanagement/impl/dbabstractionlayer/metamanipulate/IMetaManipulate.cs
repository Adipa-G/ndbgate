using System;
using System.Collections.Generic;
using System.Data;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.support;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate
{
    public interface IMetaManipulate
    {
        void Initialize(IDbConnection con);

        DbColumnType MapColumnTypeNameToType(string columnTypeName);

        string MapColumnTypeToTypeName(DbColumnType columnTypeId);

        String GetDefaultValueForType(DbColumnType columnTypeId);

        ReferentialRuleType MapReferentialRuleNameToType(string ruleTypeName);

        ICollection<IMetaItem> GetMetaData(IDbConnection con);

        ICollection<MetaQueryHolder> CreateDbPathSql(IMetaComparisonGroup metaComparisonGroup);

        bool Equals(IMetaItem iMetaItemA, IMetaItem iMetaItemB);
    }
}