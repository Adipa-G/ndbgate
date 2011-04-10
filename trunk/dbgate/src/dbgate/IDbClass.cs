namespace dbgate
{
    public interface IDbClass : IRoDbClass
    {
        DbClassStatus Status { get; set; }
    }
}