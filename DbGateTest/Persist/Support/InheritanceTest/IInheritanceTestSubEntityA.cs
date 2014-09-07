namespace DbGate.Persist.Support.InheritanceTest
{
    public interface IInheritanceTestSubEntityA : IInheritanceTestSuperEntity
    {
        string NameA { get; set; }
    }
}