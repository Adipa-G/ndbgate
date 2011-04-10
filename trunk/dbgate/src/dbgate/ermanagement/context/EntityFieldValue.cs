
namespace dbgate.ermanagement.context
{
    public class EntityFieldValue
    {
        public EntityFieldValue()
        {
        }

        public EntityFieldValue(object value, IDbColumn dbColumn)
        {
            Value = value;
            DbColumn = dbColumn;
        }

        public object Value { get; set; }

        public IDbColumn DbColumn { get; set; }
    }
}
