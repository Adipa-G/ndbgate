namespace DbGate.Context
{
    public interface IEntityContext
    {
        IChangeTracker ChangeTracker { get; }

        IReferenceStore ReferenceStore { get; }

        void DestroyReferenceStore();

        void CopyReferenceStoreFrom(IReadOnlyEntity entity);
	
	    bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys);
	
	    IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys);

        void AddToCurrentObjectGraphIndex(IReadOnlyEntity refEntity);
    }
}