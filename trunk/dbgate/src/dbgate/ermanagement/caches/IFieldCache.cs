using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.caches
{
    public interface IFieldCache
    {
        ICollection<IField> GetFields(Type entityType);

        ICollection<IDbColumn> GetDbColumns(Type entityType);

        ICollection<IDbColumn> GetKeys(Type type);

        ICollection<IDbRelation> GetDbRelations(Type entityType);

        void Register(Type entityType, ICollection<IField> fields);

        void Register(Type entityType);

        void Clear();
    }
}