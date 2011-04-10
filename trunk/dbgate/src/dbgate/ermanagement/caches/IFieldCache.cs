using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.caches
{
    public interface IFieldCache
    {
        ICollection<IField> GetFields(Type type);

        ICollection<IDbColumn> GetDbColumns(Type type);

        ICollection<IDbColumn> GetKeys(Type type);

        ICollection<IDbRelation> GetDbRelations(Type type);

        void Register(Type type, ICollection<IField> fields);

        void Register(Type type, IServerRoDbClass obj);

        void Clear();
    }
}