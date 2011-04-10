using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare
{
    public interface IMetaComparisonGroup
    {
        MetaItemType ItemType { get; }

        IMetaItem ExistingItem { get; set; }

        IMetaItem RequiredItem { get; set; }

        bool ShouldCreateInDb();

        bool ShouldDeleteFromDb();

        bool ShouldAlterInDb();
    }
}