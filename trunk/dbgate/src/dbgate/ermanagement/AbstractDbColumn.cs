using System;
using System.Text;

namespace dbgate.ermanagement
{
    public abstract class AbstractDbColumn : IDbColumn
    {
        protected AbstractDbColumn(String attributeName, DbColumnType type)
            : this(attributeName, PredictColumnName(attributeName), false, type, false, null)
        {
        }

        protected AbstractDbColumn(String attributeName, DbColumnType type, bool nullable)
            : this(attributeName, PredictColumnName(attributeName), false, nullable, type, 20, false, null)
        {
        }

        protected AbstractDbColumn(String attributeName, bool key, DbColumnType type)
            : this(attributeName, PredictColumnName(attributeName), key, type, false, null)
        {
        }

        protected AbstractDbColumn(String attributeName, bool key, bool nullable, DbColumnType type)
            : this(attributeName, PredictColumnName(attributeName), key, nullable, type, 20, false, null)
        {
        }

        protected AbstractDbColumn(String attributeName, string columnName, bool key, DbColumnType type,
                                   bool readFromSequence, ISequenceGenerator generator)
            : this(attributeName, columnName, key, false, type, 20, readFromSequence, generator)
        {
        }

        protected AbstractDbColumn(String attributeName, string columnName, bool key
                                   , bool nullable, DbColumnType type, int size, bool readFromSequence,
                                   ISequenceGenerator generator)
        {
            AttributeName = attributeName;
            ColumnName = columnName;
            Key = key;
            Nullable = nullable;
            ColumnType = type;
            Size = size;
            ReadFromSequence = readFromSequence;
            SequenceGenerator = generator;
        }

        #region IDbColumn Members

        public string AttributeName { get; set; }

        public DbColumnType ColumnType { get; set; }

        public int Size { get; set; }

        public string ColumnName { get; set; }

        public bool Key { get; set; }

        public bool SubClassCommonColumn { get; set; }

        public bool Nullable { get; set; }

        public bool ReadFromSequence { get; set; }

        public ISequenceGenerator SequenceGenerator { get; set; }

        #endregion

        private static string PredictColumnName(String attributeName)
        {
            bool previousCaps = false;
            var stringBuilder = new StringBuilder();
            char[] chars = attributeName.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char aChar = chars[i];
                if (Char.IsUpper(aChar) && i > 0)
                {
                    if (!previousCaps)
                    {
                        stringBuilder.Append("_");
                    }
                    previousCaps = true;
                }
                else
                {
                    previousCaps = false;
                }
                stringBuilder.Append(aChar);
            }
            return stringBuilder.ToString();
        }
    }
}