using DbGate.ErManagement.ErMapper.Utils;
using System.Collections.Generic;
using System.Text;

namespace DbGate.Context.Impl
{
    public class ReferenceStore : IReferenceStore
    {
        private readonly Dictionary<string, IReadOnlyEntity> entityIndex;

        public ReferenceStore()
        {
            entityIndex = new Dictionary<string, IReadOnlyEntity>();
        }

        #region IErSession Members

        public bool AlreadyInCurrentObjectGraph(ITypeFieldValueList keys)
        {
            return entityIndex.ContainsKey(BuildKey(keys));
        }

        public IReadOnlyEntity GetFromCurrentObjectGraph(ITypeFieldValueList keys)
        {
            entityIndex.TryGetValue(BuildKey(keys), out var entity);
            return entity;
        }

        public void AddToCurrentObjectGraphIndex(IReadOnlyEntity refEntity)
        {
            var refKeyList = OperationUtils.ExtractEntityKeyValues(refEntity);
            var key = BuildKey(refKeyList);
            if (!entityIndex.ContainsKey(key))
            {
                entityIndex[key] = refEntity;
            }
        }

        private static string BuildKey(ITypeFieldValueList keys)
        {
            var sb = new StringBuilder();
            sb.Append(keys.Type.FullName);
            foreach (var fv in keys.FieldValues)
            {
                sb.Append(':');
                sb.Append(fv.Column.AttributeName);
                sb.Append('=');
                sb.Append(fv.Value);
            }
            return sb.ToString();
        }
        #endregion
    }
}