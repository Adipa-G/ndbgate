namespace DbGate.Context.Impl
{
    public class EntityFieldValueList : EntityTypeFieldValueList, IEntityFieldValueList
    {
        private readonly IReadOnlyEntity _entity;

        public EntityFieldValueList(IReadOnlyEntity entity) : base(entity.GetType())
        {
            _entity = entity;
        }

        #region IEntityFieldValueList Members

        public IReadOnlyEntity Entity
        {
            get { return _entity; }
        }

        #endregion
    }
}