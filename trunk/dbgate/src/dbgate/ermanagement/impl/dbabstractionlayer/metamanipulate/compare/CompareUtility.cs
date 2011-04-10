using System.Collections.Generic;
using System.Linq;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare
{
    public class CompareUtility
    {
        public static ICollection<IMetaComparisonGroup> Compare(IMetaManipulate metaManipulate, ICollection<IMetaItem> existing, ICollection<IMetaItem> required)
        {
            List<IMetaComparisonGroup> retGroups = new List<IMetaComparisonGroup>();

            foreach (IMetaItem existingItem in existing)
            {
                bool found = false;
                foreach (IMetaItem requiredItem in required)
                {
                    if (metaManipulate.Equals(existingItem,requiredItem))
                    {
                        retGroups.Add(CreateGroupByType(metaManipulate,existingItem.ItemType,existingItem,requiredItem));
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    retGroups.Add(CreateGroupByType(metaManipulate,existingItem.ItemType,existingItem,null));
                }
            }

            foreach (IMetaItem requiredItem in required)
            {
                bool found = false;
                foreach (IMetaItem existingItem in existing)
                {
                    if (metaManipulate.Equals(requiredItem,existingItem))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    retGroups.Add(CreateGroupByType(metaManipulate,requiredItem.ItemType,null,requiredItem));
                }
            }

            return retGroups;
        }

        private static IMetaComparisonGroup CreateGroupByType(IMetaManipulate metaManipulate, MetaItemType type, IMetaItem existingItem, IMetaItem requiredItem)
        {
            switch (type)
            {
                case MetaItemType.Column:
                    return new MetaComparisonColumnGroup(existingItem, requiredItem);
                case MetaItemType.ForeignKey:
                    return new MetaComparisonForeignKeyGroup(existingItem, requiredItem);
                case MetaItemType.PrimaryKey:
                    return new MetaComparisonPrimaryKeyGroup(existingItem, requiredItem);
                case MetaItemType.Table:
                    MetaComparisonTableGroup group = new MetaComparisonTableGroup(existingItem, requiredItem);
                    ProcessTableSubGroups(metaManipulate, group);
                    return group;
                default:
                    return null;
            }
        }

        private static void ProcessTableSubGroups(IMetaManipulate metaManipulate, MetaComparisonTableGroup tableGroup)
        {
            if (tableGroup.ShouldDeleteFromDb())
            {
                return;
            }
            //columns
            if (tableGroup.ShouldAlterInDb())
            {
                List<IMetaItem> existingColumns = new List<IMetaItem>();
                List<IMetaItem> requiredColumns = new List<IMetaItem>();
                List<MetaComparisonColumnGroup> comparedColumns = new List<MetaComparisonColumnGroup>();
                
                existingColumns.AddRange(((MetaTable)tableGroup.ExistingItem).Columns.Cast<IMetaItem>());
                requiredColumns.AddRange(((MetaTable)tableGroup.RequiredItem).Columns.Cast<IMetaItem>());
                
                ICollection<IMetaComparisonGroup> compared = Compare(metaManipulate,existingColumns,requiredColumns);
                foreach (IMetaComparisonGroup columnComparison in compared)
                {
                    comparedColumns.Add((MetaComparisonColumnGroup) columnComparison);
                }
                tableGroup.Columns = comparedColumns;
            }
            
            //fkeys
            {
                List<IMetaItem> existingForeignKeys = new List<IMetaItem>();
                List<IMetaItem> requiredForeignKeys = new List<IMetaItem>();
                List<MetaComparisonForeignKeyGroup> comparedForeignKeys = new List<MetaComparisonForeignKeyGroup>();

                if (tableGroup.ExistingItem != null)
                {
                    existingForeignKeys.AddRange((((MetaTable)tableGroup.ExistingItem).ForeignKeys.Cast<IMetaItem>()));
                }
                requiredForeignKeys.AddRange(((MetaTable)tableGroup.RequiredItem).ForeignKeys.Cast<IMetaItem>());
                
                ICollection<IMetaComparisonGroup> compared = Compare(metaManipulate,existingForeignKeys,requiredForeignKeys);
                foreach (IMetaComparisonGroup columnComparison in compared)
                {
                    comparedForeignKeys.Add((MetaComparisonForeignKeyGroup) columnComparison);
                }
                tableGroup.ForeignKeys = comparedForeignKeys;
            }
            
            //pk
            {
                List<IMetaItem> existingPrimaryKey = new List<IMetaItem>();
                List<IMetaItem> requiredPrimaryKey = new List<IMetaItem>();
                List<MetaComparisonPrimaryKeyGroup> comparedPrimaryKey = new List<MetaComparisonPrimaryKeyGroup>();

                if (tableGroup.ExistingItem != null
                        && ((MetaTable)tableGroup.ExistingItem).PrimaryKey != null)
                {
                    existingPrimaryKey.Add(((MetaTable)tableGroup.ExistingItem).PrimaryKey);
                }
                if (tableGroup.RequiredItem != null
                        && ((MetaTable)tableGroup.RequiredItem).PrimaryKey != null)
                {
                    requiredPrimaryKey.Add(((MetaTable)tableGroup.RequiredItem).PrimaryKey);
                }
                
                ICollection<IMetaComparisonGroup> compared = Compare(metaManipulate,existingPrimaryKey,requiredPrimaryKey);
                foreach (IMetaComparisonGroup columnComparison in compared)
                {
                    comparedPrimaryKey.Add((MetaComparisonPrimaryKeyGroup) columnComparison);
                }
                if (comparedPrimaryKey.Count > 0)
                {
                    tableGroup.PrimaryKey = comparedPrimaryKey[0];
                }
            }
        }
    }
}
