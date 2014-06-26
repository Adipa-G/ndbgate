namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.datastructures
{
    public interface IMetaItem
    {
        string Name { get; }

        MetaItemType ItemType { get; }
    }
}