using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare
{
    public class MetaComparisonForeignKeyGroup : AbstractMetaComparisonGroup
    {
        public MetaComparisonForeignKeyGroup(IMetaItem existingItem, IMetaItem requiredItem)
            : base(existingItem, requiredItem)
        {
        }

        public override bool ShouldCreateInDb()
        {
            return (RequiredItem != null && ExistingItem == null)
                   || (RequiredItem != null && !RequiredItem.Equals(ExistingItem));
        }

        public override bool ShouldDeleteFromDb()
        {
            return (ExistingItem != null && RequiredItem == null)
                   || (ExistingItem != null && !ExistingItem.Equals(RequiredItem));
        }

        public override bool ShouldAlterInDb()
        {
            return ExistingItem != null && RequiredItem != null
 				        && !ExistingItem.Equals(RequiredItem);
        }
    }
}