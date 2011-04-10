using System;
using System.Collections.Generic;
using System.Data;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.exceptions;
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
        private readonly IErLayerConfig _config;

        public ErMetaDataManager(IDbLayer dbLayer,IErLayerConfig config)
        {
            _dbLayer = dbLayer;
            _config = config;
        }

        public void PatchDataBase(IDbConnection con, ICollection<IServerDbClass> dbClasses,bool dropAll)
        {
            try
            {
                foreach (IServerDbClass dbClass in dbClasses)
                {
                    Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(dbClass.GetType(),new []{typeof (IServerDbClass)});
                    foreach (Type type in typeList)
                    {
                        CacheManager.TableCache.Register(type,dbClass);
                        CacheManager.FieldCache.Register(type,dbClass);
                    }
                }

                IMetaManipulate metaManipulate =  _dbLayer.GetMetaManipulate(con);
                ICollection<IMetaItem> existingItems = metaManipulate.GetMetaData(con);
                ICollection<IMetaItem> requiredItems = CreateMetaItemsFromDbClasses(dbClasses);

                List<MetaQueryHolder> queryHolders = new List<MetaQueryHolder>();

                if (dropAll)
                {
                    ICollection<IMetaComparisonGroup> groupExisting = CompareUtility.Compare(metaManipulate,existingItems,new List<IMetaItem>());
                    ICollection<IMetaComparisonGroup> groupRequired = CompareUtility.Compare(metaManipulate,new List<IMetaItem>(), requiredItems);

                    List<MetaQueryHolder> queryHoldersExisting = new List<MetaQueryHolder>();
                    foreach (IMetaComparisonGroup comparisonGroup in groupExisting)
                    {
                        queryHoldersExisting.AddRange(_dbLayer.GetMetaManipulate(con).CreateDbPathSql(comparisonGroup));
                    }
                    queryHoldersExisting.Sort();

                    List<MetaQueryHolder> queryHoldersRequired = new List<MetaQueryHolder>();
                    foreach (IMetaComparisonGroup comparisonGroup in groupRequired)
                    {
                        queryHoldersRequired.AddRange(_dbLayer.GetMetaManipulate(con).CreateDbPathSql(comparisonGroup));
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
                        queryHolders.AddRange(_dbLayer.GetMetaManipulate(con).CreateDbPathSql(comparisonGroup));
                    }
                    queryHolders.Sort();
                }

                foreach (MetaQueryHolder holder in queryHolders)
                {
                    LogManager.GetLogger(_config.LoggerName).Debug(holder.QueryString);

                    IDbCommand cmd = con.CreateCommand();
                    cmd.CommandText = holder.QueryString;
                    cmd.ExecuteNonQuery();
                    DbMgmtUtility.Close(cmd);
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(_config.LoggerName).Fatal(e.Message,e);
                throw new MetaDataException(e.Message,e);
            }
        }

        private static ICollection<IMetaItem> CreateMetaItemsFromDbClasses(IEnumerable<IServerDbClass> dbClasses)
        {
            ICollection<IMetaItem> metaItems = new List<IMetaItem>();
            ICollection<String> uniqueNames = new List<String>();

            foreach (IServerDbClass serverDBClass in dbClasses)
            {
                IEnumerable<IMetaItem> classMetaItems = ExtractMetaItems(serverDBClass);
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

        private static IEnumerable<IMetaItem> ExtractMetaItems(IServerDbClass serverDBClass)
        {
            ICollection<IMetaItem> retItems = new List<IMetaItem>();

            Type[] superTypes = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(serverDBClass.GetType(),new[]{typeof(IServerDbClass)});
            foreach (Type type in superTypes)
            {
                String tableName = CacheManager.TableCache.GetTableName(type);
                if (tableName == null)
                {
                    continue;
                }
                ICollection<IField> fields = DbClassAttributeExtractionUtils.GetAllFields(serverDBClass, type);

                ICollection<IDbColumn> dbColumns = new List<IDbColumn>();
                ICollection<IDbRelation> dbRelations = new List<IDbRelation>();

                foreach (IField field in fields)
                {
                    if (field is IDbColumn)
                    {
                        dbColumns.Add((IDbColumn) field);
                    }
                    else if (field is IDbRelation)
                    {
                        IDbRelation relation = (IDbRelation) field;
                        if (!relation.ReverseRelationship
                            && !relation.NonIdentifyingRelation)
                        {
                            dbRelations.Add((IDbRelation) field);
                        }
                    }
                }

                retItems.Add(CreateTable(type,dbColumns,dbRelations));
            }
            return retItems;
        }

        private static IMetaItem CreateTable(Type type,IEnumerable<IDbColumn> dbColumns,IEnumerable<IDbRelation> dbRelations)
        {
            MetaTable table = new MetaTable();
            table.Name = CacheManager.TableCache.GetTableName(type);

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
                MetaForeignKey foreignKey = new MetaForeignKey();
                foreignKey.Name = relation.RelationShipName;
                foreignKey.ToTable = CacheManager.TableCache.GetTableName(relation.RelatedObjectType);
                foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
                {
                    string fromCol = ErDataManagerUtils.FindColumnByAttribute(CacheManager.FieldCache.GetDbColumns(type),mapping.FromField).ColumnName;
                    string toCol = ErDataManagerUtils.FindColumnByAttribute(CacheManager.FieldCache.GetDbColumns(relation.RelatedObjectType),mapping.ToField).ColumnName;
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

