namespace dbgate.ermanagement
{
    public class DefaultColumn : AbstractColumn
    {
        public DefaultColumn(string attributeName, ColumnType type) : base(attributeName, type)
        {
        }

        public DefaultColumn(string attributeName, ColumnType type, bool nullable)
            : base(attributeName, type, nullable)
        {
        }

        public DefaultColumn(string attributeName, bool key, ColumnType type) : base(attributeName, key, type)
        {
        }

        public DefaultColumn(string attributeName, bool key, bool nullable, ColumnType type)
            : base(attributeName, key, nullable, type)
        {
        }

        public DefaultColumn(string attributeName, string columnName, bool key, ColumnType type,
                               bool readFromSequence, ISequenceGenerator generator)
            : base(attributeName, columnName, key, type, readFromSequence, generator)
        {
        }

        public DefaultColumn(string attributeName, string columnName, bool key, bool nullable, ColumnType type,
                               int size, bool readFromSequence, ISequenceGenerator generator)
            : base(attributeName, columnName, key, nullable, type, size, readFromSequence, generator)
        {
        }
    }
}