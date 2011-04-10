using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare
{
    public class MetaComparisonColumnGroup : AbstractMetaComparisonGroup
    {
        public MetaComparisonColumnGroup(IMetaItem existingItem, IMetaItem requiredItem) : base(existingItem, requiredItem)
        {
        }

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
