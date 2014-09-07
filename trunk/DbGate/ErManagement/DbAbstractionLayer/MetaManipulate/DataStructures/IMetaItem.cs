namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures
{
    public interface IMetaItem
    {
        string Name { get; }

        MetaItemType ItemType { get; }
    }
}