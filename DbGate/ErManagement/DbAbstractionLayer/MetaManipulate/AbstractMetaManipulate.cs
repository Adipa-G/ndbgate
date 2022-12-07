using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Mappings;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Support;
using DbGate.Exceptions;
using log4net;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate
{
    public abstract class AbstractMetaManipulate : IMetaManipulate
    {
        protected List<ColumnTypeMapItem> ColumnTypeMapItems;
        protected List<ReferentialRuleTypeMapItem> ReferentialRuleTypeMapItems;
        protected IDbLayer DbLayer;
        protected IDbGateConfig Config;

        public AbstractMetaManipulate(IDbLayer dbLayer,IDbGateConfig config)
        {
            DbLayer = dbLayer;
            Config = config;
            ColumnTypeMapItems = new List<ColumnTypeMapItem>();
            ReferentialRuleTypeMapItems = new List<ReferentialRuleTypeMapItem>();
        }
        protected abstract ICollection<MetaTable> ExtractTableData(ITransaction tx);

        protected abstract void ExtractColumnData(ITransaction tx, MetaTable table);

        protected abstract void ExtractForeignKeyData(ITransaction tx, MetaTable table);

        protected abstract void ExtractPrimaryKeyData(ITransaction tx, MetaTable table);

        protected abstract string CreateCreateTableQuery(MetaComparisonTableGroup tableGroup);

        protected abstract string CreateDropTableQuery(MetaComparisonTableGroup tableGroup);

        protected abstract string CreateAlterTableQuery(MetaComparisonTableGroup tableGroup);

        protected abstract string CreateCreateColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup);

        protected abstract string CreateDropColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup);

        protected abstract string CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup);

        protected abstract string CreateCreatePrimaryKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonPrimaryKeyGroup primaryKeyGroup);

        protected abstract string CreateDropPrimaryKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonPrimaryKeyGroup primaryKeyGroup);

        protected abstract string CreateCreateForeginKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonForeignKeyGroup foreignKeyGroup);

        protected abstract string CreateDropForeignKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonForeignKeyGroup foreignKeyGroup);

        public void Initialize(ITransaction tx)
        {
            FillDataMappings(tx);
            FillReferentialRuleMappings(tx);
        }

        protected virtual void FillDataMappings(ITransaction tx)
        {
            try
            {
                if (tx.Connection is SqlConnection)
                {
                    var sqlConnection = (SqlConnection) tx.Connection;
                    var typeTable = sqlConnection.GetSchema();
                    if (typeTable == null)
                    {
                        Logger.GetLogger(Config.LoggerName).Fatal("Unable to read the list of types");                        
                        return;
                    }
                    foreach (DataRow dataRow in typeTable.Rows)
                    {
                        var mapItem = new ColumnTypeMapItem();
                        mapItem.Name = dataRow["TYPE_NAME"].ToString();
                        mapItem.SetTypeFromSqlType((DbType)dataRow["DATA_TYPE"]);
                        ColumnTypeMapItems.Add(mapItem);
                    }
                }
                else
                {
                    Logger.GetLogger(Config.LoggerName).Fatal("The connection is not oledb type, cannot read type info");
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal("Exception occured while trying to read type information", e);
                throw new DbPatchingException(e.Message, e);
            }  
        }

        protected virtual void FillReferentialRuleMappings(ITransaction tx)
        {
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Cascade,"0"));
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Restrict,"1"));
        }

        public ColumnType MapColumnTypeNameToType(string columnTypeName)
        {
            foreach (var typeMapItem in ColumnTypeMapItems)
            {
                if (typeMapItem.Name.Equals(columnTypeName,StringComparison.OrdinalIgnoreCase))
                {
                    return typeMapItem.ColumnType;
                }
            }
            return ColumnType.Unknown;
        }

        public string MapColumnTypeToTypeName(ColumnType columnTypeId)
        {
            foreach (var typeMapItem in ColumnTypeMapItems)
            {
                if (typeMapItem.ColumnType == columnTypeId)
                {
                    return typeMapItem.Name;
                }
            }
            return null;
        }

       
        public string GetDefaultValueForType(ColumnType columnTypeId)
        {
            foreach (var typeMapItem in ColumnTypeMapItems)
            {
                if (typeMapItem.ColumnType == columnTypeId)
                {
                    return typeMapItem.DefaultNonNullValue;
                }
            }
            return null;
        }

        public ReferentialRuleType MapReferentialRuleNameToType(String ruleTypeName)
        {
            foreach (var ruleTypeMapItem in ReferentialRuleTypeMapItems)
            {
                if (ruleTypeMapItem.RuleName.Equals(ruleTypeName,StringComparison.OrdinalIgnoreCase))
                {
                    return ruleTypeMapItem.RuleType;
                }
            }
            return ReferentialRuleType.Unknown;
        }

        public ICollection<IMetaItem> GetMetaData(ITransaction tx)
        {
            var metaTables = ExtractTableData(tx);
            ICollection<IMetaItem> metaItems = new List<IMetaItem>();

            foreach (var metaTable in metaTables)
            {
                ExtractColumnData(tx, metaTable);

                ExtractPrimaryKeyData(tx, metaTable);

                ExtractForeignKeyData(tx, metaTable); 

                metaItems.Add(metaTable);
            }
            return metaItems;
        }

        public ICollection<MetaQueryHolder> CreateDbPathSql(IMetaComparisonGroup metaComparisonGroup)
        {
            var holders = new List<MetaQueryHolder>();
            if (metaComparisonGroup is MetaComparisonTableGroup)
            {
                var tableGroup = (MetaComparisonTableGroup) metaComparisonGroup;
                if (metaComparisonGroup.ShouldCreateInDb())
                {
                    var query = CreateCreateTableQuery(tableGroup);
                    if (!string.IsNullOrEmpty(query))
                    {
                        holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeTable,
                                                        MetaQueryHolder.OperationTypeAdd, query));
                    }
                }
                if (metaComparisonGroup.ShouldDeleteFromDb())
                {
                    var query = CreateDropTableQuery(tableGroup);
                    if (!string.IsNullOrEmpty(query))
                    {
                        holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeTable,
                                                        MetaQueryHolder.OperationTypeDelete, query));
                    }
                }
                if (metaComparisonGroup.ShouldAlterInDb())
                {
                    var query = CreateAlterTableQuery(tableGroup);
                    if (!string.IsNullOrEmpty(query))
                    {
                        holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeTable,
                                                        MetaQueryHolder.OperationTypeAlter, query));
                    }
                }
                if (tableGroup.PrimaryKey != null)
                {
                    var primaryKeyGroup =  tableGroup.PrimaryKey;
                    if (primaryKeyGroup.ShouldCreateInDb()
                            || primaryKeyGroup.ShouldAlterInDb())
                    {
                        var query = CreateCreatePrimaryKeyQuery(tableGroup,primaryKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypePrimaryKey,
                                                            MetaQueryHolder.OperationTypeAdd, query));
                        }
                    }
                    if (tableGroup.ShouldDeleteFromDb()
                            || primaryKeyGroup.ShouldAlterInDb())
                    {
                        var query = CreateDropPrimaryKeyQuery(tableGroup,primaryKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypePrimaryKey,
                                                            MetaQueryHolder.OperationTypeDelete, query));
                        }
                    }
                }

                foreach (var comparisonColumnGroup in tableGroup.Columns)
                {
                    if (comparisonColumnGroup.ShouldCreateInDb())
                    {
                        var query = CreateCreateColumnQuery(tableGroup,comparisonColumnGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeColumn,
                                                            MetaQueryHolder.OperationTypeAdd, query));
                        }
                    }
                    if (comparisonColumnGroup.ShouldDeleteFromDb())
                    {
                        var query = CreateDropColumnQuery(tableGroup,comparisonColumnGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeColumn,
                                                            MetaQueryHolder.OperationTypeDelete, query));
                        }
                    }
                    if (comparisonColumnGroup.ShouldAlterInDb())
                    {
                        var query = CreateAlterColumnQuery(tableGroup,comparisonColumnGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeColumn,
                                                            MetaQueryHolder.OperationTypeAlter, query));
                        }
                    }
                }

                foreach (var foreignKeyGroup in tableGroup.ForeignKeys)
                {
                    if (foreignKeyGroup.ShouldCreateInDb()
                            || foreignKeyGroup.ShouldAlterInDb())
                    {
                        var query = CreateCreateForeginKeyQuery(tableGroup,foreignKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeForeignKey,
                                                            MetaQueryHolder.OperationTypeAdd, query));
                        }
                    }
                    if (tableGroup.ShouldDeleteFromDb()
                            || foreignKeyGroup.ShouldAlterInDb())
                    {
                        var query = CreateDropForeignKeyQuery(tableGroup,foreignKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.ObjectTypeForeignKey,
                                                            MetaQueryHolder.OperationTypeDelete, query));
                        }
                    }
                }
            }

            return holders;
        }

        public virtual bool Equals(IMetaItem iMetaItemA, IMetaItem iMetaItemB)
        {
            return (iMetaItemA.ItemType == iMetaItemB.ItemType
                            && iMetaItemA.Name.Equals(iMetaItemB.Name,StringComparison.OrdinalIgnoreCase));
        }
    }
}
