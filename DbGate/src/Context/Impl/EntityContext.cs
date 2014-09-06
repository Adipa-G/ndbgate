namespace DbGate.Context.Impl
{
    public class EntityContext : IEntityContext
    {
        private readonly IChangeTracker _changeTracker;
        private IReferenceStore _referenceStore;

        public EntityContext()
        {
            _changeTracker = new ChangeTracker();
        }

        #region IEntityContext Members

        public IChangeTracker ChangeTracker
        {
            get { return _changeTracker; }
        }

        public IReferenceStore ReferenceStore
        {
            get
            {
                InitReferenceStore();
                return _referenceStore;
            }
        }

        public void DestroyReferenceStore()
        {
            _referenceStore = null;
        }

        public void CopyReferenceStoreFrom(IReadOnlyEntity entity)
        {
            InitReferenceStore();
	        if (entity.Context != null)
	            _referenceStore = entity.Context.ReferenceStore;
        }

        public bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys)
        {
            return _referenceStore != null && _referenceStore.AlreadyInCurrentObjectGraph(keys);
        }

        public IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys)
        {
            if (_referenceStore == null)
	            return null;
	        return _referenceStore.GetFromCurrentObjectGraph(keys);
        }

        public void AddToCurrentObjectGraphIndex(IReadOnlyEntity refEntity)
        {
            InitReferenceStore();
	        _referenceStore.AddToCurrentObjectGraphIndex(refEntity);
        }

        private void InitReferenceStore()
	    {
	        if (_referenceStore == null)
	            _referenceStore = new ReferenceStore();
	    }
        #endregion
    }
}