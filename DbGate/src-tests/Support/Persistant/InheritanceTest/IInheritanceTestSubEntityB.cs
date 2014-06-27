namespace DbGate.Support.Persistant.InheritanceTest
{
    public interface IInheritanceTestSubEntityB : IInheritanceTestSuperEntity
    {
        string NameB { get; set; }
    }
}