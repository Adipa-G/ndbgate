namespace DbGate.Context.Impl
{
    public class EntityContext : IEntityContext
    {
        private readonly IChangeTracker _changeTracker;

        public EntityContext()
        {
            _changeTracker = new ChangeTracker();
        }

        #region IEntityContext Members

        public IChangeTracker ChangeTracker
        {
            get { return _changeTracker; }
        }

        public IErSession ErSession { get; set; }

        #endregion
    }
}