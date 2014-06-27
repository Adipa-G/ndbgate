using System.Collections.Generic;

namespace DbGate.Context.Impl
{
    public class ChangeTracker : IChangeTracker
    {
        private readonly ICollection<ITypeFieldValueList> _childEntityRelationKeys;
        private readonly ICollection<EntityFieldValue> _fields;

        public ChangeTracker()
        {
            _fields = new List<EntityFieldValue>();
            _childEntityRelationKeys = new List<ITypeFieldValueList>();
        }

        #region IChangeTracker Members

        public ICollection<EntityFieldValue> Fields
        {
            get { return _fields; }
        }

        public ICollection<ITypeFieldValueList> ChildEntityKeys
        {
            get { return _childEntityRelationKeys; }
        }

        public EntityFieldValue GetFieldValue(string attributeName)
        {
            foreach (EntityFieldValue fieldValue in _fields)
            {
                if (fieldValue.Column.AttributeName.Equals(attributeName))
                {
                    return fieldValue;
                }
            }
            return null;
        }

        public bool Valid
        {
            get { return Fields.Count > 0 || ChildEntityKeys.Count > 0; }
        }

        #endregion
    }
}