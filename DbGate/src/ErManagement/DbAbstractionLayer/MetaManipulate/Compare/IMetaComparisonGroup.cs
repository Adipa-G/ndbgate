using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare
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