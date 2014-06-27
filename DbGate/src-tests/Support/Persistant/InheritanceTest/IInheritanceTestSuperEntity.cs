namespace DbGate.Support.Persistant.InheritanceTest
{
    public interface IInheritanceTestSuperEntity : IEntity
    {
        int IdCol { get; set; }

        string Name { get; set; }
    }
}