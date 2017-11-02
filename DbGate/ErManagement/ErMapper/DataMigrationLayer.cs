using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Support;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.Exceptions.Migration;
using DbGate.Utility;
using log4net;

namespace DbGate.ErManagement.ErMapper
{
    public class DataMigrationLayer
    {
        private readonly IDbLayer _dbLayer;
        private readonly IDbGateStatistics _statistics;
        private readonly IDbGateConfig _config;

        public DataMigrationLayer(IDbLayer dbLayer,IDbGateStatistics statistics,IDbGateConfig config)
        {
            _dbLayer = dbLayer;
            _statistics = statistics;
            _config = config;
        }

        public void PatchDataBase(ITransaction tx, ICollection<Type> entityTypes,bool dropAll)
        {
            try
            {
                foreach (Type entityType in entityTypes)
                {
                    CacheManager.Register(entityType);
                }

                IMetaManipulate metaManipulate =  _dbLayer.MetaManipulate(tx);
                ICollection<IMetaItem> existingItems = metaManipulate.GetMetaData(tx);
                ICollection<IMetaItem> requiredItems = CreateMetaItemsFromEntityTypes(entityTypes);

                List<MetaQueryHolder> queryHolders = new List<MetaQueryHolder>();

                if (dropAll)
                {
                    ICollection<IMetaComparisonGroup> groupExisting = CompareUtility.Compare(metaManipulate,existingItems,new List<IMetaItem>());
                    ICollection<IMetaComparisonGroup> groupRequired = CompareUtility.Compare(metaManipulate,new List<IMetaItem>(), requiredItems);

                    List<MetaQueryHolder> queryHoldersExisting = new List<MetaQueryHolder>();
                    foreach (IMetaComparisonGroup comparisonGroup in groupExisting)
                    {
                        queryHoldersExisting.AddRange(_dbLayer.MetaManipulate(tx).CreateDbPathSql(comparisonGroup));
                    }
                    queryHoldersExisting.Sort();

                    List<MetaQueryHolder> queryHoldersRequired = new List<MetaQueryHolder>();
                    foreach (IMetaComparisonGroup comparisonGroup in groupRequired)
                    {
                        queryHoldersRequired.AddRange(_dbLayer.MetaManipulate(tx).CreateDbPathSql(comparisonGroup));
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
                        queryHolders.AddRange(_dbLayer.MetaManipulate(tx).CreateDbPathSql(comparisonGroup));
                    }
                    queryHolders.Sort();
                }

                foreach (MetaQueryHolder holder in queryHolders)
                {
                    if (holder.QueryString == null)
                        continue;

                    Logger.GetLogger( _config.LoggerName).Debug(holder.QueryString);

                    IDbCommand cmd = tx.CreateCommand();
                    cmd.CommandText = holder.QueryString;
                    cmd.ExecuteNonQuery();
                    DbMgtUtility.Close(cmd);

                    if (_config.EnableStatistics) _statistics.RegisterPatch();
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger( _config.LoggerName).Fatal(e.Message,e);
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
                ICollection<IColumn> dbColumns = entityInfo.Columns;
                ICollection<IRelation> dbRelations = entityInfo.Relations;
                var filteredRelations = new List<IRelation>();

                foreach (IRelation relation in dbRelations)
                {
                    if (relation.NonIdentifyingRelation)
                    {
                        filteredRelations.Add(relation);
                    }
                }

                var oneSideReverse = CacheManager.GetReversedRelationships(subType);
                foreach (var relation in oneSideReverse)
                {
                    if (filteredRelations.All(r => r.RelationShipName != relation.RelationShipName))
                    {
                        var reversed = relation.Clone();
                        reversed.RelatedObjectType = relation.SourceObjectType;

                        var orgMappings = reversed.TableColumnMappings.ToList();
                        reversed.TableColumnMappings = orgMappings
                            .Select(m => new RelationColumnMapping(m.ToField, m.FromField)).ToList();
                        filteredRelations.Add(reversed);
                    }
                }
                
                retItems.Add(CreateTable(entityInfo.EntityType,dbColumns, filteredRelations));
                entityInfo = entityInfo.SuperEntityInfo;
            }
            return retItems;
        }

        private static IMetaItem CreateTable(Type type,IEnumerable<IColumn> dbColumns,IEnumerable<IRelation> dbRelations)
        {
            MetaTable table = new MetaTable();
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            table.Name = entityInfo.TableInfo.TableName;

            foreach (IColumn dbColumn in dbColumns)
            {
                MetaColumn metaColumn = new MetaColumn();
                metaColumn.ColumnType = dbColumn.ColumnType;
                metaColumn.Name = dbColumn.ColumnName;
                metaColumn.Null = dbColumn.Nullable;
                metaColumn.Size = dbColumn.Size;
                table.Columns.Add(metaColumn);
            }

            foreach (IRelation relation in dbRelations)
            {
                EntityInfo relatedEntityInfo = CacheManager.GetEntityInfo(relation.RelatedObjectType);

                MetaForeignKey foreignKey = new MetaForeignKey();
                foreignKey.Name = relation.RelationShipName;
                foreignKey.ToTable = relatedEntityInfo.TableInfo.TableName;
                foreach (RelationColumnMapping mapping in relation.TableColumnMappings)
                {
                    string fromCol = entityInfo.FindColumnByAttribute(mapping.FromField).ColumnName;
                    string toCol = relatedEntityInfo.FindColumnByAttribute(mapping.ToField).ColumnName;
                    foreignKey.ColumnMappings.Add(new MetaForeignKeyColumnMapping(fromCol,toCol));
                }
                foreignKey.DeleteRule = relation.DeleteRule;
                foreignKey.UpdateRule = relation.UpdateRule;
                table.ForeignKeys.Add(foreignKey);
            }

            MetaPrimaryKey primaryKey = new MetaPrimaryKey();
            primaryKey.Name = "pk_" + table.Name;
            foreach (IColumn dbColumn in dbColumns)
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

