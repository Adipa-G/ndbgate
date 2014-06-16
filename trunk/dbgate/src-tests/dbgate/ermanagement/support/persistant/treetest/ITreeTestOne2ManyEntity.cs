namespace dbgate.ermanagement.support.persistant.treetest
{
    public interface ITreeTestOne2ManyEntity  : IEntity
    {
        int IdCol { get; set; }

        int IndexNo { get; set; }

        string Name { get; set; }
    }
}
