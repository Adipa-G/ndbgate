using System;
using System.Collections.Generic;

namespace DbGate.Context.Impl
{
    public class EntityTypeFieldValueList : ITypeFieldValueList
    {
        private readonly ICollection<EntityFieldValue> fieldValues;

        public EntityTypeFieldValueList(Type type)
        {
            Type = type;
            fieldValues = new List<EntityFieldValue>();
        }

        #region ITypeFieldValueList Members

        public Type Type { get; private set; }

        public ICollection<EntityFieldValue> FieldValues => fieldValues;

        public EntityFieldValue GetFieldValue(String attributeName)
        {
            foreach (var fieldValue in fieldValues)
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