using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.context.impl
{
    public class EntityRelationFieldValueList : ITypeFieldValueList
    {
        private readonly IRelation _relation;
        private readonly ICollection<EntityFieldValue> _fieldValues;

        public EntityRelationFieldValueList(IRelation relation)
        {
            _relation = relation;
            _fieldValues = new List<EntityFieldValue>();
        }

        public IRelation Relation 
        {
            get { return _relation; }
        }

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
    }
}