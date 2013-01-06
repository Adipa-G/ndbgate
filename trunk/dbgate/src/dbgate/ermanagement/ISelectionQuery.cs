using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace dbgate.ermanagement
{
    public interface ISelectionQuery : IQuery
    {
		ISelectionQuery From(IQueryFrom queryFrom);
		 	
		ISelectionQuery Join(IQueryJoin queryJoin);
		 	
		ISelectionQuery Where(IQueryCondition queryCondition);

        ICollection<object> ToList(IDbConnection con);
   
        ISelectionQuery Select(IQuerySelection querySelection);

        ISelectionQuery GroupBy(IQueryGroup queryGroup);

        ISelectionQuery OrderBy(IQueryOrderBy queryOrderBy);

        ISelectionQuery Having(IQueryGroupCondition queryGroupCondition);
    }
}