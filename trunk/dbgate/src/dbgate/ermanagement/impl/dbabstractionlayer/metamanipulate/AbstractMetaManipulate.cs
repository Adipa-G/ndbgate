using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.mappings;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.support;
using log4net;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate
{
    public abstract class AbstractMetaManipulate : IMetaManipulate
    {
        protected List<ColumnTypeMapItem> ColumnTypeMapItems;
        protected List<ReferentialRuleTypeMapItem> ReferentialRuleTypeMapItems;
        protected IDbLayer DBLayer;
        protected IErLayerConfig Config;

        public AbstractMetaManipulate(IDbLayer dbLayer,IErLayerConfig config)
        {
            DBLayer = dbLayer;
            Config = config;
            ColumnTypeMapItems = new List<ColumnTypeMapItem>();
            ReferentialRuleTypeMapItems = new List<ReferentialRuleTypeMapItem>();
        }
        protected abstract ICollection<MetaTable> ExtractTableData(IDbConnection connection);

        protected abstract void ExtractColumnData(IDbConnection connection, MetaTable table);

        protected abstract void ExtractForeignKeyData(IDbConnection connection, MetaTable table);

        protected abstract void ExtractPrimaryKeyData(IDbConnection connection, MetaTable table);

        protected abstract string CreateCreateTableQuery(MetaComparisonTableGroup tableGroup);

        protected abstract string CreateDropTableQuery(MetaComparisonTableGroup tableGroup);

        protected abstract string CreateAlterTableQuery(MetaComparisonTableGroup tableGroup);

        protected abstract string CreateCreateColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup);

        protected abstract string CreateDropColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup);

        protected abstract string CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup);

        protected abstract string CreateCreatePrimaryKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonPrimaryKeyGroup primaryKeyGroup);

        protected abstract string CreateDropPrimaryKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonPrimaryKeyGroup primaryKeyGroup);

        protected abstract string CreateCreateForeginKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonForeignKeyGroup foreignKeyGroup);

        protected abstract string CreateDropForeginKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonForeignKeyGroup foreignKeyGroup);

        public void Initialize(IDbConnection con)
        {
            FillDataMappings(con);
            FillReferentialRuleMappings(con);
        }

        protected virtual void FillDataMappings(IDbConnection con)
        {
            try
            {
                if (con is OleDbConnection)
                {
                    OleDbConnection oleDbConnection = (OleDbConnection) con;
                    DataTable typeTable = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Provider_Types, null);
                    if (typeTable == null)
                    {
                        LogManager.GetLogger(Config.LoggerName).Fatal("Unable to read the list of types");                        
                        return;
                    }
                    foreach (DataRow dataRow in typeTable.Rows)
                    {
                        ColumnTypeMapItem mapItem = new ColumnTypeMapItem();
                        mapItem.Name = dataRow["TYPE_NAME"].ToString();
                        mapItem.SetTypeFromSqlType((DbType)dataRow["DATA_TYPE"]);
                        ColumnTypeMapItems.Add(mapItem);
                    }
                }
                else
                {
                    LogManager.GetLogger(Config.LoggerName).Fatal("The connection is not oledb type, cannot read type info");
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal("Exception occured while trying to read type information", e);
                throw new DBPatchingException(e.Message, e);
            }  
        }

        protected void FillReferentialRuleMappings(IDbConnection con)
        {
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Cascade,"0"));
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Restrict,"1"));
        }

        public DbColumnType MapColumnTypeNameToType(string columnTypeName)
        {
            foreach (ColumnTypeMapItem typeMapItem in ColumnTypeMapItems)
            {
                if (typeMapItem.Name.Equals(columnTypeName,StringComparison.OrdinalIgnoreCase))
                {
                    return typeMapItem.ColumnType;
                }
            }
            return DbColumnType.Unknown;
        }

        public string MapColumnTypeToTypeName(DbColumnType columnTypeId)
        {
            foreach (ColumnTypeMapItem typeMapItem in ColumnTypeMapItems)
            {
                if (typeMapItem.ColumnType == columnTypeId)
                {
                    return typeMapItem.Name;
                }
            }
            return null;
        }

       
        public string GetDefaultValueForType(DbColumnType columnTypeId)
        {
            foreach (ColumnTypeMapItem typeMapItem in ColumnTypeMapItems)
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
            foreach (ReferentialRuleTypeMapItem ruleTypeMapItem in ReferentialRuleTypeMapItems)
            {
                if (ruleTypeMapItem.RuleName.Equals(ruleTypeName,StringComparison.OrdinalIgnoreCase))
                {
                    return ruleTypeMapItem.RuleType;
                }
            }
            return ReferentialRuleType.Unknown;
        }

        public ICollection<IMetaItem> GetMetaData(IDbConnection con)
        {
            ICollection<MetaTable> metaTables = ExtractTableData(con);
            ICollection<IMetaItem> metaItems = new List<IMetaItem>();

            foreach (MetaTable metaTable in metaTables)
            {
                ExtractColumnData(con, metaTable);

                ExtractPrimaryKeyData(con, metaTable);

                ExtractForeignKeyData(con, metaTable); 

                metaItems.Add(metaTable);
            }
            return metaItems;
        }

        public ICollection<MetaQueryHolder> CreateDbPathSql(IMetaComparisonGroup metaComparisonGroup)
        {
            List<MetaQueryHolder> holders = new List<MetaQueryHolder>();
            if (metaComparisonGroup is MetaComparisonTableGroup)
            {
                MetaComparisonTableGroup tableGroup = (MetaComparisonTableGroup) metaComparisonGroup;
                if (metaComparisonGroup.ShouldCreateInDb())
                {
                    string query = CreateCreateTableQuery(tableGroup);
                    if (!string.IsNullOrEmpty(query))
                    {
                        holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_TABLE,MetaQueryHolder.OPERATION_TYPE_ADD, query));
                    }
                }
                if (metaComparisonGroup.ShouldDeleteFromDb())
                {
                    string query = CreateDropTableQuery(tableGroup);
                    if (!string.IsNullOrEmpty(query))
                    {
                        holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_TABLE,MetaQueryHolder.OPERATION_TYPE_DELETE, query));
                    }
                }
                if (metaComparisonGroup.ShouldAlterInDb())
                {
                    string query = CreateAlterTableQuery(tableGroup);
                    if (!string.IsNullOrEmpty(query))
                    {
                        holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_TABLE,MetaQueryHolder.OPERATION_TYPE_ALTER, query));
                    }
                }
                if (tableGroup.PrimaryKey != null)
                {
                    MetaComparisonPrimaryKeyGroup primaryKeyGroup =  tableGroup.PrimaryKey;
                    if (primaryKeyGroup.ShouldCreateInDb()
                            || primaryKeyGroup.ShouldAlterInDb())
                    {
                        string query = CreateCreatePrimaryKeyQuery(tableGroup,primaryKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_PRIMARY_KEY,MetaQueryHolder.OPERATION_TYPE_ADD, query));
                        }
                    }
                    if (tableGroup.ShouldDeleteFromDb()
                            || primaryKeyGroup.ShouldAlterInDb())
                    {
                        string query = CreateDropPrimaryKeyQuery(tableGroup,primaryKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_PRIMARY_KEY,MetaQueryHolder.OPERATION_TYPE_DELETE, query));
                        }
                    }
                }

                foreach (MetaComparisonColumnGroup comparisonColumnGroup in tableGroup.Columns)
                {
                    if (comparisonColumnGroup.ShouldCreateInDb())
                    {
                        string query = CreateCreateColumnQuery(tableGroup,comparisonColumnGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_COLUMN,MetaQueryHolder.OPERATION_TYPE_ADD, query));
                        }
                    }
                    if (comparisonColumnGroup.ShouldDeleteFromDb())
                    {
                        string query = CreateDropColumnQuery(tableGroup,comparisonColumnGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_COLUMN,MetaQueryHolder.OPERATION_TYPE_DELETE, query));
                        }
                    }
                    if (comparisonColumnGroup.ShouldAlterInDb())
                    {
                        string query = CreateAlterColumnQuery(tableGroup,comparisonColumnGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_COLUMN,MetaQueryHolder.OPERATION_TYPE_ALTER, query));
                        }
                    }
                }

                foreach (MetaComparisonForeignKeyGroup foreignKeyGroup in tableGroup.ForeignKeys)
                {
                    if (foreignKeyGroup.ShouldCreateInDb()
                            || foreignKeyGroup.ShouldAlterInDb())
                    {
                        string query = CreateCreateForeginKeyQuery(tableGroup,foreignKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_FOREIGN_KEY,MetaQueryHolder.OPERATION_TYPE_ADD, query));
                        }
                    }
                    if (tableGroup.ShouldDeleteFromDb()
                            || foreignKeyGroup.ShouldAlterInDb())
                    {
                        string query = CreateDropForeginKeyQuery(tableGroup,foreignKeyGroup);
                        if (!string.IsNullOrEmpty(query))
                        {
                            holders.Add(new MetaQueryHolder(MetaQueryHolder.OBJECT_TYPE_FOREIGN_KEY,MetaQueryHolder.OPERATION_TYPE_DELETE, query));
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
