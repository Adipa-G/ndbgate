using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DbGate.Context.Impl
{
    public class ChangeTracker : IChangeTracker
    {
        private ReadOnlyCollection<ITypeFieldValueList> childEntityRelationKeys;
        private ReadOnlyCollection<EntityFieldValue> fields;

        public ChangeTracker()
        {
            fields = new ReadOnlyCollection<EntityFieldValue>(new List<EntityFieldValue>());
            childEntityRelationKeys = new ReadOnlyCollection<ITypeFieldValueList>(new List<ITypeFieldValueList>());
        }

        #region IChangeTracker Members

        public IEnumerable<ITypeFieldValueList> ChildEntityKeys => childEntityRelationKeys;

        public EntityFieldValue GetFieldValue(string attributeName)
        {
            foreach (var fieldValue in fields)
            {
                if (fieldValue.Column.AttributeName.Equals(attributeName))
                {
                    return fieldValue;
                }
            }
            return null;
        }

        public bool Valid => fields.Count > 0 || childEntityRelationKeys.Count > 0;

        public void AddChildEntityKey(ITypeFieldValueList list)
        {
            lock (this)
            {
                var tmpList = new List<ITypeFieldValueList>();
                tmpList.AddRange(childEntityRelationKeys);
                tmpList.Add(list);

                childEntityRelationKeys = new ReadOnlyCollection<ITypeFieldValueList>(tmpList);    
            }  
        }

        public void AddFields(ICollection<EntityFieldValue> fieldValues)
        {
            lock (this)
            {
                var toAdd = new List<EntityFieldValue>();

                var tmpList = new List<EntityFieldValue>();
                tmpList.AddRange(fields);

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

                fields = new ReadOnlyCollection<EntityFieldValue>(tmpList);
            }
        }

        #endregion
    }
}