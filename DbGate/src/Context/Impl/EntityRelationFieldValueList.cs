using System;
using System.Collections.Generic;

namespace DbGate.Context.Impl
{
    public class EntityRelationFieldValueList : ITypeFieldValueList
    {
        private readonly ICollection<EntityFieldValue> _fieldValues;
        private readonly IRelation _relation;

        public EntityRelationFieldValueList(IRelation relation)
        {
            _relation = relation;
            _fieldValues = new List<EntityFieldValue>();
        }

        public IRelation Relation
        {
            get { return _relation; }
        }

        #region ITypeFieldValueList Members

        public Type Type
        {
            get { return _relation.RelatedObjectType; }
        }

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