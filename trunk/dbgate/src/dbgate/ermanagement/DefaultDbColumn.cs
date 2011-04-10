namespace dbgate.ermanagement
{
    public class DefaultDbColumn : AbstractDbColumn
    {
        public DefaultDbColumn(string attributeName, DbColumnType type) : base(attributeName, type)
        {
        }

        public DefaultDbColumn(string attributeName, DbColumnType type, bool nullable)
            : base(attributeName, type, nullable)
        {
        }

        public DefaultDbColumn(string attributeName, bool key, DbColumnType type) : base(attributeName, key, type)
        {
        }

        public DefaultDbColumn(string attributeName, bool key, bool nullable, DbColumnType type)
            : base(attributeName, key, nullable, type)
        {
        }

        public DefaultDbColumn(string attributeName, string columnName, bool key, DbColumnType type,
                               bool readFromSequence, ISequenceGenerator generator)
            : base(attributeName, columnName, key, type, readFromSequence, generator)
        {
        }

        public DefaultDbColumn(string attributeName, string columnName, bool key, bool nullable, DbColumnType type,
                               int size, bool readFromSequence, ISequenceGenerator generator)
            : base(attributeName, columnName, key, nullable, type, size, readFromSequence, generator)
        {
        }
    }
}