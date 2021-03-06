﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using DbGate.ErManagement.ErMapper;

namespace DbGate.ErManagement.Query
{
    public class SelectionQuery : Query , ISelectionQuery
    {
        public ICollection<object> ToList(ITransaction tx)
        {
            return tx.DbGate.Select(this, tx);
        }
        
     	public ISelectionQuery Distinct()
        {
			Structure.Distinct = true;
			return this;
        }

		public ISelectionQuery Fetch(long records)
        {
			Structure.Fetch = records;
			return this;
        }

		public ISelectionQuery Skip(long records)
        {
			Structure.Skip = records;
			return this;
        }

		public ISelectionQuery From(IQueryFrom queryFrom)
        {
            return (ISelectionQuery)base.From(queryFrom);
        }

        public new ISelectionQuery Join(IQueryJoin queryJoin)
        {
            return (ISelectionQuery)base.Join(queryJoin);
        }

        public new ISelectionQuery Where(IQueryCondition queryCondition)
        {
            return (ISelectionQuery)base.Where(queryCondition);
        }

        public ISelectionQuery Select(IQuerySelection querySelection)
        {
            Structure.SelectList.Add(querySelection);
            return this;
        }

        public ISelectionQuery GroupBy(IQueryGroup queryGroup)
        {
            Structure.GroupList.Add(queryGroup);
            return this;
        }

        public ISelectionQuery OrderBy(IQueryOrderBy queryOrderBy)
        {
            Structure.OrderList.Add(queryOrderBy);
            return this;
        }

        public ISelectionQuery Having(IQueryGroupCondition queryGroupCondition)
        {
            Structure.GroupConditionList.Add(queryGroupCondition);
            return this;
        }
    }
}
