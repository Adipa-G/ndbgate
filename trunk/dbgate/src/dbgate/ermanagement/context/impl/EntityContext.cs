namespace dbgate.ermanagement.context.impl
{
    public class EntityContext : IEntityContext
    {
        private readonly IChangeTracker _changeTracker;

        public EntityContext()
        {
            _changeTracker = new ChangeTracker();
        }

        public IChangeTracker ChangeTracker
        {
            get { return _changeTracker; }
        }

        public IErSession ErSession { get; set; }
    }
}
