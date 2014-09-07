using System.Collections.Generic;
using System.Data;
using DbGate.ErManagement.Query;

namespace DbGate
{
    public interface ISelectionQuery : IQuery
    {
        ISelectionQuery Distinct();

        ISelectionQuery Skip(long records);

        ISelectionQuery Fetch(long records);

        new ISelectionQuery From(IQueryFrom queryFrom);

        new ISelectionQuery Join(IQueryJoin queryJoin);

        new ISelectionQuery Where(IQueryCondition queryCondition);

        ICollection<object> ToList(ITransaction tx);

        ISelectionQuery Select(IQuerySelection querySelection);

        ISelectionQuery GroupBy(IQueryGroup queryGroup);

        ISelectionQuery OrderBy(IQueryOrderBy queryOrderBy);

        ISelectionQuery Having(IQueryGroupCondition queryGroupCondition);
    }
}