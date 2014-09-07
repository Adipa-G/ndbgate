using System.Collections.Generic;
using DbGate.ErManagement.ErMapper.Utils;

namespace DbGate.Context.Impl
{
    public class ReferenceStore : IReferenceStore
    {
        private readonly ICollection<IEntityFieldValueList> _entityFieldValueList;

        public ReferenceStore()
        {
            _entityFieldValueList = new List<IEntityFieldValueList>();
        }

        #region IErSession Members

        public bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys)
        {
            return GetFromCurrentObjectGraph(keys) != null;
        }

        public IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys)
        {
            foreach (IEntityFieldValueList existingEntity in _entityFieldValueList)
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
            IEntityFieldValueList refKeyList = OperationUtils.ExtractEntityKeyValues(refEntity);
            if (!AlreadyInCurrentObjectGraph(refKeyList))
            {
                _entityFieldValueList.Add(refKeyList);
            }
        }
        #endregion
    }
}