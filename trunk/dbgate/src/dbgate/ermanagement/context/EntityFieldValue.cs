
namespace dbgate.ermanagement.context
{
    public class EntityFieldValue
    {
        public EntityFieldValue()
        {
        }

        public EntityFieldValue(object value, IColumn column)
        {
            Value = value;
            Column = column;
        }

        public object Value { get; set; }

        public IColumn Column { get; set; }
    }
}
