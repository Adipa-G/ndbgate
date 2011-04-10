using System.Collections.Generic;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures
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
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
