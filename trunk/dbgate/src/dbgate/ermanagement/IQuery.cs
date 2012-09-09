using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IQuery
    {
        IQuery From(IQueryFrom queryFrom);

        IQuery Join(IQueryJoin queryJoin);

        IQuery Where(IQueryCondition queryCondition);

        QueryStructure Structure { get; }
    }
}
