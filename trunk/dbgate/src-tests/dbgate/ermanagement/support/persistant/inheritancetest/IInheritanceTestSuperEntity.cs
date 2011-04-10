namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    public interface IInheritanceTestSuperEntity : IServerDbClass
    {
        int IdCol { get; set; }

        string Name { get; set; }
    }
}
