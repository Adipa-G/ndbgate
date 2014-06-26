using dbgate.ermanagement.dbabstractionlayer.metamanipulate.datastructures;

namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.compare
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