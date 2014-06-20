namespace dbgate.support.persistant.inheritancetest
{
    public interface IInheritanceTestSubEntityB : IInheritanceTestSuperEntity
    {
        string NameB { get; set; }
    }
}