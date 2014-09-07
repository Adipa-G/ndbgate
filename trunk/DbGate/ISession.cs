namespace DbGate
{
    public interface ISession
    {
        void StartTransaction();

        void CommitTransaction();

        void Save(IClientEntity clientEntity);
    }
}