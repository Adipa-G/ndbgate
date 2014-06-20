using System;

namespace dbgate
{
    [AttributeUsage(AttributeTargets.All)]
    public class ForeignKeyColumnMapping : Attribute
    {
        public readonly string FromField;
        public readonly string ToField;

        public ForeignKeyColumnMapping(string fromField, string toField)
        {
            FromField = fromField;
            ToField = toField;
        }
    }
}