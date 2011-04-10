using System;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures
{
    public class MetaForeignKeyColumnMapping
    {
        public MetaForeignKeyColumnMapping(string fromColumn, string toColumn)
        {
            FromColumn = fromColumn;
            ToColumn = toColumn;
        }

        public string FromColumn { get; set; }

        public string ToColumn { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            MetaForeignKeyColumnMapping that = (MetaForeignKeyColumnMapping) o;

            if (FromColumn != null && !FromColumn.Equals(that.FromColumn,StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (ToColumn != null && !ToColumn.Equals(that.ToColumn,StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
