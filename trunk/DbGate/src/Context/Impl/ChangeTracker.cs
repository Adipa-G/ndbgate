using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DbGate.Context.Impl
{
    public class ChangeTracker : IChangeTracker
    {
        private ReadOnlyCollection<ITypeFieldValueList> _childEntityRelationKeys;
        private ReadOnlyCollection<EntityFieldValue> _fields;

        public ChangeTracker()
        {
            _fields = new ReadOnlyCollection<EntityFieldValue>(new List<EntityFieldValue>());
            _childEntityRelationKeys = new ReadOnlyCollection<ITypeFieldValueList>(new List<ITypeFieldValueList>());
        }

        #region IChangeTracker Members

        public IEnumerable<EntityFieldValue> Fields
        {
            get { return _fields; }
        }

        public IEnumerable<ITypeFieldValueList> ChildEntityKeys
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
            get { return _fields.Count > 0 || _childEntityRelationKeys.Count > 0; }
        }

        public void AddChildEntityKey(ITypeFieldValueList list)
        {
            lock (this)
            {
                var tmpList = new List<ITypeFieldValueList>();
                tmpList.AddRange(_childEntityRelationKeys);
                tmpList.Add(list);

                _childEntityRelationKeys = new ReadOnlyCollection<ITypeFieldValueList>(tmpList);    
            }  
        }

        public void AddFields(ICollection<EntityFieldValue> fieldValues)
        {
            lock (this)
            {
                var toAdd = new List<EntityFieldValue>();

                var tmpList = new List<EntityFieldValue>();
                tmpList.AddRange(_fields);

                foreach (var newFieldValue in fieldValues)
                {
                    var value = tmpList.FirstOrDefault(l => l.Column.AttributeName.Equals(newFieldValue.Column.AttributeName));
                    if (value != null)
                    {
                        value.Value = newFieldValue.Value;
                    }
                    else
                    {
                        toAdd.Add(newFieldValue);
                    }
                }
                tmpList.AddRange(toAdd);

                _fields = new ReadOnlyCollection<EntityFieldValue>(tmpList);
            }
        }

        #endregion
    }
}