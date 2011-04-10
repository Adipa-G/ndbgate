namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures
{
    public interface IMetaItem
    {
        string Name { get; }

        MetaItemType ItemType { get; }
    }
}