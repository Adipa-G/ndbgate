namespace dbgate.ermanagement.context.impl
{
    public class EntityFieldValueList : EntityTypeFieldValueList , IEntityFieldValueList
    {
        private readonly IServerRoDbClass _entity;

        public EntityFieldValueList(IServerRoDbClass entity) : base (entity.GetType())
        {
            _entity = entity;
        }

        public IServerRoDbClass Entity
        {
            get {  return _entity; }
        }
    }
}