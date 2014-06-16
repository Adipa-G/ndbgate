using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dbgate.ermanagement.caches.impl;

namespace dbgate.ermanagement.caches
{
    public interface IEntityInfoCache
    {
        EntityInfo GetEntityInfo(Type entityType);

        EntityInfo GetEntityInfo(IReadOnlyClientEntity entity);

        void Register(Type entityType,String tableName,ICollection<IField> fields);

        void Register(Type entityType);
    
        void Clear();
    }
}
