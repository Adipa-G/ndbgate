namespace DbGate.Context.Impl
{
    public class EntityContext : IEntityContext
    {
        private readonly IChangeTracker changeTracker;
        private IReferenceStore referenceStore;

        public EntityContext()
        {
            changeTracker = new ChangeTracker();
        }

        #region IEntityContext Members

        public IChangeTracker ChangeTracker => changeTracker;

        public IReferenceStore ReferenceStore
        {
            get
            {
                InitReferenceStore();
                return referenceStore;
            }
        }

        public void DestroyReferenceStore()
        {
            referenceStore = null;
        }

        public void CopyReferenceStoreFrom(IReadOnlyEntity entity)
        {
            if (entity.Context != null)
	            referenceStore = entity.Context.ReferenceStore;
        }

        public bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys)
        {
            InitReferenceStore();
            return referenceStore.AlreadyInCurrentObjectGraph(keys);
        }

        public IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys)
        {
            InitReferenceStore();
	        return referenceStore.GetFromCurrentObjectGraph(keys);
        }

        public void AddToCurrentObjectGraphIndex(IReadOnlyEntity refEntity)
        {
            InitReferenceStore();
	        referenceStore.AddToCurrentObjectGraphIndex(refEntity);
        }

        private void InitReferenceStore()
	    {
	        if (referenceStore == null)
	            referenceStore = new ReferenceStore();
	    }
        #endregion
    }
}