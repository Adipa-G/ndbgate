using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using System.Collections.Generic;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare
{
    public class MetaComparisonTableGroup : AbstractMetaComparisonGroup
    {
        public MetaComparisonTableGroup(IMetaItem existingItem, IMetaItem requiredItem)
            : base(existingItem, requiredItem)
        {
            Columns = new List<MetaComparisonColumnGroup>();
            ForeignKeys = new List<MetaComparisonForeignKeyGroup>();
        }

        public ICollection<MetaComparisonColumnGroup> Columns { get; set; }

        public ICollection<MetaComparisonForeignKeyGroup> ForeignKeys { get; set; }

        public MetaComparisonPrimaryKeyGroup PrimaryKey { get; set; }

        public override bool ShouldCreateInDb()
        {
            return RequiredItem != null && ExistingItem == null;
        }

        public override bool ShouldDeleteFromDb()
        {
            return ExistingItem != null && RequiredItem == null;
        }

        public override bool ShouldAlterInDb()
        {
            return ExistingItem != null && RequiredItem != null && !ExistingItem.Equals(RequiredItem);
        }
    }
}