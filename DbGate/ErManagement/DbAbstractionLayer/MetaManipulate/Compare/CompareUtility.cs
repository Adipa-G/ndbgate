using System.Collections.Generic;
using System.Linq;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare
{
    public class CompareUtility
    {
        public static ICollection<IMetaComparisonGroup> Compare(IMetaManipulate metaManipulate,
                                                                ICollection<IMetaItem> existing,
                                                                ICollection<IMetaItem> required)
        {
            var retGroups = new List<IMetaComparisonGroup>();

            foreach (IMetaItem existingItem in existing)
            {
                bool found = false;
                foreach (IMetaItem requiredItem in required)
                {
                    if (metaManipulate.Equals(existingItem, requiredItem))
                    {
                        retGroups.Add(CreateGroupByType(metaManipulate, existingItem.ItemType, existingItem,
                                                        requiredItem));
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    retGroups.Add(CreateGroupByType(metaManipulate, existingItem.ItemType, existingItem, null));
                }
            }

            foreach (IMetaItem requiredItem in required)
            {
                bool found = false;
                foreach (IMetaItem existingItem in existing)
                {
                    if (metaManipulate.Equals(requiredItem, existingItem))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    retGroups.Add(CreateGroupByType(metaManipulate, requiredItem.ItemType, null, requiredItem));
                }
            }

            return retGroups;
        }

        private static IMetaComparisonGroup CreateGroupByType(IMetaManipulate metaManipulate, MetaItemType type,
                                                              IMetaItem existingItem, IMetaItem requiredItem)
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
                    var group = new MetaComparisonTableGroup(existingItem, requiredItem);
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
                var existingColumns = new List<IMetaItem>();
                var requiredColumns = new List<IMetaItem>();
                var comparedColumns = new List<MetaComparisonColumnGroup>();

                existingColumns.AddRange(((MetaTable) tableGroup.ExistingItem).Columns.Cast<IMetaItem>());
                requiredColumns.AddRange(((MetaTable) tableGroup.RequiredItem).Columns.Cast<IMetaItem>());

                ICollection<IMetaComparisonGroup> compared = Compare(metaManipulate, existingColumns, requiredColumns);
                foreach (IMetaComparisonGroup columnComparison in compared)
                {
                    comparedColumns.Add((MetaComparisonColumnGroup) columnComparison);
                }
                tableGroup.Columns = comparedColumns;
            }

            //fkeys
            {
                var existingForeignKeys = new List<IMetaItem>();
                var requiredForeignKeys = new List<IMetaItem>();
                var comparedForeignKeys = new List<MetaComparisonForeignKeyGroup>();

                if (tableGroup.ExistingItem != null)
                {
                    existingForeignKeys.AddRange((((MetaTable) tableGroup.ExistingItem).ForeignKeys.Cast<IMetaItem>()));
                }
                requiredForeignKeys.AddRange(((MetaTable) tableGroup.RequiredItem).ForeignKeys.Cast<IMetaItem>());

                ICollection<IMetaComparisonGroup> compared = Compare(metaManipulate, existingForeignKeys,
                                                                     requiredForeignKeys);
                foreach (IMetaComparisonGroup columnComparison in compared)
                {
                    comparedForeignKeys.Add((MetaComparisonForeignKeyGroup) columnComparison);
                }
                tableGroup.ForeignKeys = comparedForeignKeys;
            }

            //pk
            {
                var existingPrimaryKey = new List<IMetaItem>();
                var requiredPrimaryKey = new List<IMetaItem>();
                var comparedPrimaryKey = new List<MetaComparisonPrimaryKeyGroup>();

                if (tableGroup.ExistingItem != null
                    && ((MetaTable) tableGroup.ExistingItem).PrimaryKey != null)
                {
                    existingPrimaryKey.Add(((MetaTable) tableGroup.ExistingItem).PrimaryKey);
                }
                if (tableGroup.RequiredItem != null
                    && ((MetaTable) tableGroup.RequiredItem).PrimaryKey != null)
                {
                    requiredPrimaryKey.Add(((MetaTable) tableGroup.RequiredItem).PrimaryKey);
                }

                ICollection<IMetaComparisonGroup> compared = Compare(metaManipulate, existingPrimaryKey,
                                                                     requiredPrimaryKey);
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