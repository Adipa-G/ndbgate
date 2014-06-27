using System.Collections.Generic;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures
{
    public class MetaTable : AbstractMetaItem
    {
        public MetaTable()
        {
            ItemType = MetaItemType.Table;
            Columns = new List<MetaColumn>();
            ForeignKeys = new List<MetaForeignKey>();
        }

        public MetaPrimaryKey PrimaryKey { get; set; }

        public ICollection<MetaColumn> Columns { get; set; }

        public ICollection<MetaForeignKey> ForeignKeys { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            if (!base.Equals(o)) return false;

            var that = (MetaTable) o;

            if (PrimaryKey == null && that.PrimaryKey != null) return false;
            if (PrimaryKey != null && that.PrimaryKey == null) return false;
            if (PrimaryKey != null && that.PrimaryKey != null
                && !PrimaryKey.Equals(that.PrimaryKey)) return false;

            foreach (MetaForeignKey foreignKey in ForeignKeys)
            {
                bool found = false;
                foreach (MetaForeignKey thatForeignKey in that.ForeignKeys)
                {
                    if (foreignKey.Name.Equals(thatForeignKey.Name))
                    {
                        found = true;
                        if (!foreignKey.Equals(thatForeignKey)) return false;
                    }
                }
                if (!found) return false;
            }

            foreach (MetaForeignKey thatForeignKey in that.ForeignKeys)
            {
                bool found = false;
                foreach (MetaForeignKey foreignKey in ForeignKeys)
                {
                    if (foreignKey.Name.Equals(thatForeignKey.Name))
                    {
                        found = true;
                    }
                }
                if (!found) return false;
            }

            foreach (MetaColumn column in Columns)
            {
                bool found = false;
                foreach (MetaColumn thatColumn in that.Columns)
                {
                    if (column.Name.Equals(thatColumn.Name))
                    {
                        found = true;
                        if (!column.Equals(thatColumn)) return false;
                    }
                }
                if (!found) return false;
            }

            foreach (MetaColumn thatColumn in that.Columns)
            {
                bool found = false;
                foreach (MetaColumn column in Columns)
                {
                    if (column.Name.Equals(thatColumn.Name))
                    {
                        found = true;
                    }
                }
                if (!found) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}