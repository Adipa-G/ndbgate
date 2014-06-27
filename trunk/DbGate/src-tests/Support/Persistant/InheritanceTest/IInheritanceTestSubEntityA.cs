namespace DbGate.Support.Persistant.InheritanceTest
{
    public interface IInheritanceTestSubEntityA : IInheritanceTestSuperEntity
    {
        string NameA { get; set; }
    }
}