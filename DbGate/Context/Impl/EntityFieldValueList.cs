namespace DbGate.Context.Impl
{
    public class EntityFieldValueList : EntityTypeFieldValueList, IEntityFieldValueList
    {
        private readonly IReadOnlyEntity entity;

        public EntityFieldValueList(IReadOnlyEntity entity) : base(entity.GetType())
        {
            this.entity = entity;
        }

        #region IEntityFieldValueList Members

        public IReadOnlyEntity Entity => entity;

        #endregion
    }
}