using System.Collections.Generic;
using DbGate.ErManagement.ErMapper.Utils;

namespace DbGate.Context.Impl
{
    public class ReferenceStore : IReferenceStore
    {
        private readonly ICollection<IEntityFieldValueList> entityFieldValueList;

        public ReferenceStore()
        {
            entityFieldValueList = new List<IEntityFieldValueList>();
        }

        #region IErSession Members

        public bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys)
        {
            return GetFromCurrentObjectGraph(keys) != null;
        }

        public IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys)
        {
            foreach (var existingEntity in entityFieldValueList)
            {
                if (OperationUtils.IsTypeKeyEquals(keys, existingEntity))
                {
                    return  existingEntity.Entity;
                }
            }
            return null;
        }

        public void AddToCurrentObjectGraphIndex(IReadOnlyEntity refEntity)
        {
            var refKeyList = OperationUtils.ExtractEntityKeyValues(refEntity);
            if (!AlreadyInCurrentObjectGraph(refKeyList))
            {
                entityFieldValueList.Add(refKeyList);
            }
        }
        #endregion
    }
}