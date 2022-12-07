using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DbGate.ErManagement.Query
{
    public class QueryStructure
    {
        private readonly ICollection<IQueryCondition> conditionList;
        private readonly ICollection<IQueryFrom> fromList;
        private readonly ICollection<IQueryGroupCondition> groupConditionList;
        private readonly ICollection<IQueryGroup> groupList;
        private readonly ICollection<IQueryJoin> joinList;
        private readonly ICollection<IQueryOrderBy> orderList;
        private readonly ICollection<IQuerySelection> selectList;

        public QueryStructure()
        {
            QueryId = Guid.NewGuid().ToString();
            fromList = new Collection<IQueryFrom>();
            joinList = new Collection<IQueryJoin>();
            conditionList = new Collection<IQueryCondition>();
            selectList = new Collection<IQuerySelection>();
            groupList = new Collection<IQueryGroup>();
            orderList = new Collection<IQueryOrderBy>();
            groupConditionList = new Collection<IQueryGroupCondition>();
        }

        public bool Distinct { get; set; }

        public long Fetch { get; set; }

        public long Skip { get; set; }

        public string QueryId { get; private set; }

        public ICollection<IQueryFrom> FromList => fromList;

        public ICollection<IQueryJoin> JoinList => joinList;

        public ICollection<IQueryCondition> ConditionList => conditionList;

        public ICollection<IQuerySelection> SelectList => selectList;

        public ICollection<IQueryGroup> GroupList => groupList;

        public ICollection<IQueryOrderBy> OrderList => orderList;

        public ICollection<IQueryGroupCondition> GroupConditionList => groupConditionList;
    }
}