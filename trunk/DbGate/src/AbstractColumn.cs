using System;
using System.Text;

namespace DbGate
{
    public abstract class AbstractColumn : IColumn
    {
        protected AbstractColumn(String attributeName, ColumnType type)
            : this(attributeName, PredictColumnName(attributeName), false, type, false, null)
        {
        }

        protected AbstractColumn(String attributeName, ColumnType type, bool nullable)
            : this(attributeName, PredictColumnName(attributeName), false, nullable, type, 20, false, null)
        {
        }

        protected AbstractColumn(String attributeName, bool key, ColumnType type)
            : this(attributeName, PredictColumnName(attributeName), key, type, false, null)
        {
        }

        protected AbstractColumn(String attributeName, bool key, bool nullable, ColumnType type)
            : this(attributeName, PredictColumnName(attributeName), key, nullable, type, 20, false, null)
        {
        }

        protected AbstractColumn(String attributeName, string columnName, bool key, ColumnType type,
                                   bool readFromSequence, ISequenceGenerator generator)
            : this(attributeName, columnName, key, false, type, 20, readFromSequence, generator)
        {
        }

        protected AbstractColumn(String attributeName, string columnName, bool key
                                   , bool nullable, ColumnType type, int size, bool readFromSequence,
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

        #region IColumn Members

        public string AttributeName { get; set; }

        public ColumnType ColumnType { get; set; }

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