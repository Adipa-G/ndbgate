using System;
using System.Collections.Generic;

namespace DbGate.Context.Impl
{
    public class EntityRelationFieldValueList : ITypeFieldValueList
    {
        private readonly ICollection<EntityFieldValue> fieldValues;
        private readonly IRelation relation;

        public EntityRelationFieldValueList(IRelation relation)
        {
            this.relation = relation;
            fieldValues = new List<EntityFieldValue>();
        }

        public IRelation Relation => relation;

        #region ITypeFieldValueList Members

        public Type Type => relation.RelatedObjectType;

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