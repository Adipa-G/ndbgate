using System.Collections.Generic;
using DbGate.ErManagement.ErMapper.Utils;

namespace DbGate.Context.Impl
{
    public class ErSession : IErSession
    {
        private readonly ICollection<IEntityFieldValueList> _entityFieldValueList;

        public ErSession()
        {
            _entityFieldValueList = new List<IEntityFieldValueList>();
        }

        #region IErSession Members

        public ICollection<IEntityFieldValueList> ProcessedObjects
        {
            get { return _entityFieldValueList; }
        }

        public IReadOnlyEntity GetProcessed(ITypeFieldValueList typeKeyFieldList)
        {
            foreach (IEntityFieldValueList existingEntity in _entityFieldValueList)
            {
                if (OperationUtils.IsTypeKeyEquals(typeKeyFieldList, existingEntity))
                {
                    return existingEntity.Entity;
                }
            }
            return null;
        }

        public bool IsProcessed(ITypeFieldValueList typeKeyFieldList)
        {
            return GetProcessed(typeKeyFieldList) != null;
        }

        public void CheckAndAddEntityList(IEntityFieldValueList entityKeyFieldList)
        {
            if (!IsProcessed(entityKeyFieldList))
            {
                _entityFieldValueList.Add((entityKeyFieldList));
            }
        }

        #endregion
    }
}