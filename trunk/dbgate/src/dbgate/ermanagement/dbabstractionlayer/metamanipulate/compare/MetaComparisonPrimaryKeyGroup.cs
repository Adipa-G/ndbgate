using dbgate.ermanagement.dbabstractionlayer.metamanipulate.datastructures;

namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.compare
{
    public class MetaComparisonPrimaryKeyGroup : AbstractMetaComparisonGroup
    {
        public MetaComparisonPrimaryKeyGroup(IMetaItem existingItem, IMetaItem requiredItem) : base(existingItem, requiredItem)
        {
        }

        public override bool ShouldCreateInDb()
        {
            return (RequiredItem != null && ExistingItem == null)
                    || (RequiredItem != null && !RequiredItem.Equals(ExistingItem) );
        }

        public override bool ShouldDeleteFromDb()
        {
            return (ExistingItem != null && RequiredItem == null)
                    || (ExistingItem != null && !ExistingItem.Equals(RequiredItem) );
        }

        public override bool ShouldAlterInDb()
        {
            return false;
        }
    }
}