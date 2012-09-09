using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace dbgate.ermanagement.query
{
    public class QueryStructure
    {
        private ICollection<IQueryFrom> _fromList;
	  	private ICollection<IQueryJoin> _joinList;
	  	private ICollection<IQueryCondition> _conditionList;
	  	private ICollection<IQuerySelection> _selectList;
	  	private ICollection<IQueryGroup> _groupList;
	  	private ICollection<IQueryOrderBy> _orderList;
	  	private ICollection<IQueryGroupCondition> _groupConditionList;
	  	 	
	  	public QueryStructure()
	  	{
	  	    _fromList = new Collection<IQueryFrom>();
            _joinList = new Collection<IQueryJoin>();
            _conditionList = new Collection<IQueryCondition>();
            _selectList = new Collection<IQuerySelection>();
            _groupList = new Collection<IQueryGroup>();
            _orderList = new Collection<IQueryOrderBy>();
            _groupConditionList = new Collection<IQueryGroupCondition>();
	  	}

        public ICollection<IQueryFrom> FromList
        {
            get { return _fromList; }
        }

        public ICollection<IQueryJoin> JoinList
        {
            get { return _joinList; }
        }

        public ICollection<IQueryCondition> ConditionList
        {
            get { return _conditionList; }
        }

        public ICollection<IQuerySelection> SelectList
        {
            get { return _selectList; }
        }

        public ICollection<IQueryGroup> GroupList
        {
            get { return _groupList; }
        }

        public ICollection<IQueryOrderBy> OrderList
        {
            get { return _orderList; }
        }

        public ICollection<IQueryGroupCondition> GroupConditionList
        {
            get { return _groupConditionList; }
        }
    }
}