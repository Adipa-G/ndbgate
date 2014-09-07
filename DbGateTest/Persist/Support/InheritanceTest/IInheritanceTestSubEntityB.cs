namespace DbGate.Persist.Support.InheritanceTest
{
    public interface IInheritanceTestSubEntityB : IInheritanceTestSuperEntity
    {
        string NameB { get; set; }
    }
}