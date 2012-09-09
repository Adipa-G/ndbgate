namespace dbgate.ermanagement.query
{
    public class Query : IQuery
    {
        public Query()
        {
            Structure = new QueryStructure();
        }
        
        public IQuery From(IQueryFrom queryFrom)
        {
            Structure.FromList.Add(queryFrom);
            return this;
        }

        public IQuery Join(IQueryJoin queryJoin)
        {
            Structure.JoinList.Add(queryJoin);
            return this;
        }

        public virtual IQuery Where(IQueryCondition queryCondition)
        {
            Structure.ConditionList.Add(queryCondition);
            return this;
        }

        public QueryStructure Structure { get; private set; }
    }
}
