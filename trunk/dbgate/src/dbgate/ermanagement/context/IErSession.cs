using System.Collections.Generic;

namespace dbgate.ermanagement.context
{
    public interface IErSession
    {
        ICollection<IEntityFieldValueList> ProcessedObjects { get; }

        bool IsProcessed(ITypeFieldValueList typeKeyFieldList);

        IReadOnlyEntity GetProcessed(ITypeFieldValueList typeKeyFieldList);

        void CheckAndAddEntityList(IEntityFieldValueList entityKeyFieldList);
    }
}
