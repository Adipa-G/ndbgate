namespace DbGate.Support.Persistant.TreeTest
{
    public interface ITreeTestOne2OneEntity : IEntity
    {
        int IdCol { get; set; }

        string Name { get; set; }
    }
}