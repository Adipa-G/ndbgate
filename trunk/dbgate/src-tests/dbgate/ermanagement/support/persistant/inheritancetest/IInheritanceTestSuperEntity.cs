namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    public interface IInheritanceTestSuperEntity : IEntity
    {
        int IdCol { get; set; }

        string Name { get; set; }
    }
}
