using dbgate.ermanagement.query;

namespace dbgate
{
    public interface IQuery
    {
        IQuery From(IQueryFrom queryFrom);

        IQuery Join(IQueryJoin queryJoin);

        IQuery Where(IQueryCondition queryCondition);

        QueryStructure Structure { get; }
    }
}
