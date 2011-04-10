namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    public interface IInheritanceTestSubEntityB : IInheritanceTestSuperEntity
    {
        string NameB { get; set; }
    }
}