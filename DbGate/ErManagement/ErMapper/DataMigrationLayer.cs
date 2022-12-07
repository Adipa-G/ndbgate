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
        private readonly IDbLayer dbLayer;
        private readonly IDbGateStatistics statistics;
        private readonly IDbGateConfig config;

        public DataMigrationLayer(IDbLayer dbLayer,IDbGateStatistics statistics,IDbGateConfig config)
        {
            this.dbLayer = dbLayer;
            this.statistics = statistics;
            this.config = config;
        }

        public void PatchDataBase(ITransaction tx, ICollection<Type> entityTypes,bool dropAll)
        {
            try
            {
                foreach (var entityType in entityTypes)
                {
                    CacheManager.Register(entityType);
                }

                var metaManipulate =  dbLayer.MetaManipulate(tx);
                var existingItems = metaManipulate.GetMetaData(tx);
                var requiredItems = CreateMetaItemsFromEntityTypes(entityTypes);

                var queryHolders = new List<MetaQueryHolder>();

                if (dropAll)
                {
                    var groupExisting = CompareUtility.Compare(metaManipulate,existingItems,new List<IMetaItem>());
                    var groupRequired = CompareUtility.Compare(metaManipulate,new List<IMetaItem>(), requiredItems);

                    var queryHoldersExisting = new List<MetaQueryHolder>();
                    foreach (var comparisonGroup in groupExisting)
                    {
                        queryHoldersExisting.AddRange(dbLayer.MetaManipulate(tx).CreateDbPathSql(comparisonGroup));
                    }
                    queryHoldersExisting.Sort();

                    var queryHoldersRequired = new List<MetaQueryHolder>();
                    foreach (var comparisonGroup in groupRequired)
                    {
                        queryHoldersRequired.AddRange(dbLayer.MetaManipulate(tx).CreateDbPathSql(comparisonGroup));
                    }
                    queryHoldersRequired.Sort();

                    queryHolders.AddRange(queryHoldersExisting);
                    queryHolders.AddRange(queryHoldersRequired);    
                }
                else
                {
                    var groups = CompareUtility.Compare(metaManipulate,existingItems,requiredItems);
                    foreach (var comparisonGroup in groups)
                    {
                        queryHolders.AddRange(dbLayer.MetaManipulate(tx).CreateDbPathSql(comparisonGroup));
                    }
                    queryHolders.Sort();
                }

                foreach (var holder in queryHolders)
                {
                    if (holder.QueryString == null)
                        continue;

                    Logger.GetLogger( config.LoggerName).Debug(holder.QueryString);

                    var cmd = tx.CreateCommand();
                    cmd.CommandText = holder.QueryString;
                    cmd.ExecuteNonQuery();
                    DbMgtUtility.Close(cmd);

                    if (config.EnableStatistics) statistics.RegisterPatch();
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger( config.LoggerName).Fatal(e.Message,e);
                throw new MetaDataException(e.Message,e);
            }
        }

        private static ICollection<IMetaItem> CreateMetaItemsFromEntityTypes(IEnumerable<Type> entityTypes)
        {
            ICollection<IMetaItem> metaItems = new List<IMetaItem>();
            ICollection<String> uniqueNames = new List<String>();

            foreach (var entityType in entityTypes)
            {
                var classMetaItems = ExtractMetaItems(entityType);
                foreach (var metaItem in classMetaItems)
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
            var entityInfo = CacheManager.GetEntityInfo(subType);

            while (entityInfo != null)
            {
                var dbColumns = entityInfo.Columns;
                var dbRelations = entityInfo.Relations;
                var filteredRelations = new List<IRelation>();

                foreach (var relation in dbRelations)
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
            var table = new MetaTable();
            var entityInfo = CacheManager.GetEntityInfo(type);
            table.Name = entityInfo.TableInfo.TableName;

            foreach (var dbColumn in dbColumns)
            {
                var metaColumn = new MetaColumn();
                metaColumn.ColumnType = dbColumn.ColumnType;
                metaColumn.Name = dbColumn.ColumnName;
                metaColumn.Null = dbColumn.Nullable;
                metaColumn.Size = dbColumn.Size;
                table.Columns.Add(metaColumn);
            }

            foreach (var relation in dbRelations)
            {
                var relatedEntityInfo = CacheManager.GetEntityInfo(relation.RelatedObjectType);

                var foreignKey = new MetaForeignKey();
                foreignKey.Name = relation.RelationShipName;
                foreignKey.ToTable = relatedEntityInfo.TableInfo.TableName;
                foreach (var mapping in relation.TableColumnMappings)
                {
                    var fromCol = entityInfo.FindColumnByAttribute(mapping.FromField).ColumnName;
                    var toCol = relatedEntityInfo.FindColumnByAttribute(mapping.ToField).ColumnName;
                    foreignKey.ColumnMappings.Add(new MetaForeignKeyColumnMapping(fromCol,toCol));
                }
                foreignKey.DeleteRule = relation.DeleteRule;
                foreignKey.UpdateRule = relation.UpdateRule;
                table.ForeignKeys.Add(foreignKey);
            }

            var primaryKey = new MetaPrimaryKey();
            primaryKey.Name = "pk_" + table.Name;
            foreach (var dbColumn in dbColumns)
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

