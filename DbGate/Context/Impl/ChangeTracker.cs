using System.Collections.Generic;
using System.Linq;

namespace DbGate.Context.Impl
{
    public class ChangeTracker : IChangeTracker
    {
        private readonly List<ITypeFieldValueList> childEntityRelationKeys;
        private readonly Dictionary<string, EntityFieldValue> fieldsByAttribute;

        public ChangeTracker()
        {
            fieldsByAttribute = new Dictionary<string, EntityFieldValue>();
            childEntityRelationKeys = new List<ITypeFieldValueList>();
        }

        #region IChangeTracker Members

        public IEnumerable<ITypeFieldValueList> ChildEntityKeys => childEntityRelationKeys;

        public EntityFieldValue GetFieldValue(string attributeName)
        {
            fieldsByAttribute.TryGetValue(attributeName, out var fieldValue);
            return fieldValue;
        }

        public bool Valid => fieldsByAttribute.Count > 0 || childEntityRelationKeys.Count > 0;

        public void AddChildEntityKey(ITypeFieldValueList list)
        {
            lock (childEntityRelationKeys)
            {
                childEntityRelationKeys.Add(list);
            }
        }

        public void AddFields(ICollection<EntityFieldValue> fieldValues)
        {
            lock (fieldsByAttribute)
            {
                foreach (var newFieldValue in fieldValues)
                {
                    fieldsByAttribute[newFieldValue.Column.AttributeName] = newFieldValue;
                }
            }
        }

        public void Clear()
        {
            lock (fieldsByAttribute)
            {
                fieldsByAttribute.Clear();
            }
            lock (childEntityRelationKeys)
            {
                childEntityRelationKeys.Clear();
            }
        }

        #endregion
    }
}