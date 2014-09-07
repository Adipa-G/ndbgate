using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DbGate.ErManagement.Query
{
    public class QueryStructure
    {
        private readonly ICollection<IQueryCondition> _conditionList;
        private readonly ICollection<IQueryFrom> _fromList;
        private readonly ICollection<IQueryGroupCondition> _groupConditionList;
        private readonly ICollection<IQueryGroup> _groupList;
        private readonly ICollection<IQueryJoin> _joinList;
        private readonly ICollection<IQueryOrderBy> _orderList;
        private readonly ICollection<IQuerySelection> _selectList;

        public QueryStructure()
        {
            QueryId = Guid.NewGuid().ToString();
            _fromList = new Collection<IQueryFrom>();
            _joinList = new Collection<IQueryJoin>();
            _conditionList = new Collection<IQueryCondition>();
            _selectList = new Collection<IQuerySelection>();
            _groupList = new Collection<IQueryGroup>();
            _orderList = new Collection<IQueryOrderBy>();
            _groupConditionList = new Collection<IQueryGroupCondition>();
        }

        public bool Distinct { get; set; }

        public long Fetch { get; set; }

        public long Skip { get; set; }

        public string QueryId { get; private set; }

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