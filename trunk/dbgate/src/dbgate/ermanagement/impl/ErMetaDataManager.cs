using System;
using System.Collections.Generic;
using System.Data;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.caches.impl;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.exceptions.migration;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.support;
using dbgate.ermanagement.impl.utils;
using log4net;

namespace dbgate.ermanagement.impl
{
    public class ErMetaDataManager
    {
        private readonly IDbLayer _dbLayer;
        private readonly IErLayerStatistics _statistics;
        private readonly IErLayerConfig _config;

        public ErMetaDataManager(IDbLayer dbLayer,IErLayerStatistics statistics,IErLayerConfig config)
        {
            _dbLayer = dbLayer;
            _statistics = statistics;
            _config = config;
        }

        public void PatchDataBase(IDbConnection con, ICollection<Type> entityTypes,bool dropAll)
        {
            try
            {
                foreach (Type entityType in entityTypes)
                {
                    CacheManager.Register(entityType);
                }

                IMetaManipulate metaManipulate =  _dbLayer.MetaManipulate(con);
                ICollection<IMetaItem> existingItems = metaManipulate.GetMetaData(con);
                ICollection<IMetaItem> requiredItems = CreateMetaItemsFromEntityTypes(entityTypes);

                List<MetaQueryHolder> queryHolders = new List<MetaQueryHolder>();

                if (dropAll)
                {
                    ICollection<IMetaComparisonGroup> groupExisting = CompareUtility.Compare(metaManipulate,existingItems,new List<IMetaItem>());
                    ICollection<IMetaComparisonGroup> groupRequired = CompareUtility.Compare(metaManipulate,new List<IMetaItem>(), requiredItems);

                    List<MetaQueryHolder> queryHoldersExisting = new List<MetaQueryHolder>();
                    foreach (IMetaComparisonGroup comparisonGroup in groupExisting)
                    {
                        queryHoldersExisting.AddRange(_dbLayer.MetaManipulate(con).CreateDbPathSql(comparisonGroup));
                    }
                    queryHoldersExisting.Sort();

                    List<MetaQueryHolder> queryHoldersRequired = new List<MetaQueryHolder>();
                    foreach (IMetaComparisonGroup comparisonGroup in groupRequired)
                    {
                        queryHoldersRequired.AddRange(_dbLayer.MetaManipulate(con).CreateDbPathSql(comparisonGroup));
                    }
                    queryHoldersRequired.Sort();

                    queryHolders.AddRange(queryHoldersExisting);
                    queryHolders.AddRange(queryHoldersRequired);    
                }
                else
                {
                    ICollection<IMetaComparisonGroup> groups = CompareUtility.Compare(metaManipulate,existingItems,requiredItems);
                    foreach (IMetaComparisonGroup comparisonGroup in groups)
                    {
                        queryHolders.AddRange(_dbLayer.MetaManipulate(con).CreateDbPathSql(comparisonGroup));
                    }
                    queryHolders.Sort();
                }

                foreach (MetaQueryHolder holder in queryHolders)
                {
                    if (holder.QueryString == null)
                        continue;

                    LogManager.GetLogger(_config.LoggerName).Debug(holder.QueryString);

                    IDbCommand cmd = con.CreateCommand();
                    cmd.CommandText = holder.QueryString;
                    cmd.ExecuteNonQuery();
                    DbMgmtUtility.Close(cmd);

                    if (_config.EnableStatistics) _statistics.RegisterPatch();
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(_config.LoggerName).Fatal(e.Message,e);
                throw new MetaDataException(e.Message,e);
            }
        }

        private static ICollection<IMetaItem> CreateMetaItemsFromEntityTypes(IEnumerable<Type> entityTypes)
        {
            ICollection<IMetaItem> metaItems = new List<IMetaItem>();
            ICollection<String> uniqueNames = new List<String>();

            foreach (Type entityType in entityTypes)
            {
                IEnumerable<IMetaItem> classMetaItems = ExtractMetaItems(entityType);
                foreach (IMetaItem metaItem in classMetaItems)
                {
                    //this is to remove duplicate tables in case of different sub classes inheriting same superclass
                    if (!uniqueNames.Contains(metaItem.Name))
                    {
                        metaItems.Add(metaItem);
                        uniqueNames.Add(metaItem.Name);
                    }
                }
            }
            return metaItems;
        }

        private static IEnumerable<IMetaItem> ExtractMetaItems(Type subType)
        {
            ICollection<IMetaItem> retItems = new List<IMetaItem>();
            EntityInfo entityInfo = CacheManager.GetEntityInfo(subType);

            while (entityInfo != null)
            {
                ICollection<IDbColumn> dbColumns = entityInfo.Columns;
                ICollection<IDbRelation> dbRelations = entityInfo.Relations;
                var filteredRelations = new List<IDbRelation>();

                foreach (IDbRelation relation in dbRelations)
                {
                    if (!relation.ReverseRelationship
                        && !relation.NonIdentifyingRelation)
                    {
                        filteredRelations.Add(relation);
                    }
                }
                
                retItems.Add(CreateTable(entityInfo.EntityType,dbColumns,dbRelations));
                entityInfo = entityInfo.SuperEntityInfo;
            }
            return retItems;
        }

        private static IMetaItem CreateTable(Type type,IEnumerable<IDbColumn> dbColumns,IEnumerable<IDbRelation> dbRelations)
        {
            MetaTable table = new MetaTable();
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            table.Name = entityInfo.TableName;

            foreach (IDbColumn dbColumn in dbColumns)
            {
                MetaColumn metaColumn = new MetaColumn();
                metaColumn.ColumnType = dbColumn.ColumnType;
                metaColumn.Name = dbColumn.ColumnName;
                metaColumn.Null = dbColumn.Nullable;
                metaColumn.Size = dbColumn.Size;
                table.Columns.Add(metaColumn);
            }

            foreach (IDbRelation relation in dbRelations)
            {
                EntityInfo relatedEntityInfo = CacheManager.GetEntityInfo(relation.RelatedObjectType);

                MetaForeignKey foreignKey = new MetaForeignKey();
                foreignKey.Name = relation.RelationShipName;
                foreignKey.ToTable = relatedEntityInfo.TableName;
                foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
                {
                    string fromCol = ErDataManagerUtils.FindColumnByAttribute(relatedEntityInfo.Columns,mapping.FromField).ColumnName;
                    string toCol = ErDataManagerUtils.FindColumnByAttribute(relatedEntityInfo.Columns,mapping.ToField).ColumnName;
                    foreignKey.ColumnMappings.Add(new MetaForeignKeyColumnMapping(fromCol,toCol));
                }
                foreignKey.DeleteRule = relation.DeleteRule;
                foreignKey.UpdateRule = relation.UpdateRule;
                table.ForeignKeys.Add(foreignKey);
            }

            MetaPrimaryKey primaryKey = new MetaPrimaryKey();
            primaryKey.Name = "pk_" + table.Name;
            foreach (IDbColumn dbColumn in dbColumns)
            {
                if (dbColumn.Key)
                {
                    primaryKey.ColumnNames.Add(dbColumn.ColumnName);
                }
            }
            if (primaryKey.ColumnNames.Count > 0)
            {
                table.PrimaryKey = primaryKey;
            }
            return table;
        }
    }
}

