﻿using System.Collections.Generic;
using dbgate.ermanagement.impl.utils;

namespace dbgate.ermanagement.context.impl
{
    public class ErSession : IErSession
    {
        private ICollection<IEntityFieldValueList> _entityFieldValueList;

        public ErSession()
        {
            _entityFieldValueList = new List<IEntityFieldValueList>();
        }

        public ICollection<IEntityFieldValueList> ProcessedObjects
        {
            get { return _entityFieldValueList; }
        }

        public IServerRoDbClass GetProcessed(ITypeFieldValueList typeKeyFieldList)
        {
            foreach (IEntityFieldValueList existingEntity in _entityFieldValueList)
            {
                if (ErDataManagerUtils.IsTypeKeyEquals(typeKeyFieldList, existingEntity))
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
    }
}