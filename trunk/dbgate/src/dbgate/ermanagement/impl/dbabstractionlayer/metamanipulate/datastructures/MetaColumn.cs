namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures
{
    public class MetaColumn : AbstractMetaItem
    {
        public MetaColumn()
        {
            ItemType = MetaItemType.Column;   
        }

        public DbColumnType ColumnType { get; set; }

        public int Size { get; set; }

        public bool Null { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            if (!base.Equals(o)) return false;

            MetaColumn that = (MetaColumn) o;

            if (Null != that.Null) return false;
            if (ColumnType != that.ColumnType) return false;
            if (Size != that.Size) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
