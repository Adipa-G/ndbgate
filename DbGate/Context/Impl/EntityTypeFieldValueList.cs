using System;
using System.Collections.Generic;

namespace DbGate.Context.Impl
{
    public class EntityTypeFieldValueList : ITypeFieldValueList
    {
        private readonly ICollection<EntityFieldValue> _fieldValues;

        public EntityTypeFieldValueList(Type type)
        {
            Type = type;
            _fieldValues = new List<EntityFieldValue>();
        }

        #region ITypeFieldValueList Members

        public Type Type { get; private set; }

        public ICollection<EntityFieldValue> FieldValues
        {
            get { return _fieldValues; }
        }

        public EntityFieldValue GetFieldValue(String attributeName)
        {
            foreach (EntityFieldValue fieldValue in _fieldValues)
            {
                if (fieldValue.Column.AttributeName.Equals(attributeName))
                {
                    return fieldValue;
                }
            }
            return null;
        }

        #endregion
    }
}