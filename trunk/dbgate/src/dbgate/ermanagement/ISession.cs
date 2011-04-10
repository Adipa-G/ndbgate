namespace dbgate.ermanagement
{
    public interface ISession
    {
        void StartTransaction();

        void CommitTransaction();

        void Save(IDbClass dbClass);
    }
}