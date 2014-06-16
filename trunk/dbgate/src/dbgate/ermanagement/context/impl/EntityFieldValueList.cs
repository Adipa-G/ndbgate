namespace dbgate.ermanagement.context.impl
{
    public class EntityFieldValueList : EntityTypeFieldValueList , IEntityFieldValueList
    {
        private readonly IReadOnlyEntity _entity;

        public EntityFieldValueList(IReadOnlyEntity entity) : base (entity.GetType())
        {
            _entity = entity;
        }

        public IReadOnlyEntity Entity
        {
            get {  return _entity; }
        }
    }
}