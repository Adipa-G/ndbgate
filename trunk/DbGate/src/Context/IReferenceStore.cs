using System.Collections.Generic;

namespace DbGate.Context
{
    public interface IReferenceStore
    {
        bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys);

        IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys);

        void AddToCurrentObjectGraphIndex(IReadOnlyEntity refEntity);
    }
}