using DbGate.ErManagement.Query;

namespace DbGate
{
    public interface IQuery
    {
        QueryStructure Structure { get; }
        IQuery From(IQueryFrom queryFrom);

        IQuery Join(IQueryJoin queryJoin);

        IQuery Where(IQueryCondition queryCondition);
    }
}