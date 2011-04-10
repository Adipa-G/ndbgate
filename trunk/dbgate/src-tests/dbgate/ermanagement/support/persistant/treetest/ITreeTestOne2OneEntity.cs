namespace dbgate.ermanagement.support.persistant.treetest
{
    public interface ITreeTestOne2OneEntity : IServerDbClass
    {
        int IdCol { get; set; }

        string Name { get; set; }
    }
}