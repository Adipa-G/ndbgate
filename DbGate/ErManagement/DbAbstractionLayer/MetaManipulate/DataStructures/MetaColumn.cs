namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures
{
    public class MetaColumn : AbstractMetaItem
    {
        public MetaColumn()
        {
            ItemType = MetaItemType.Column;
        }

        public ColumnType ColumnType { get; set; }

        public int Size { get; set; }

        public bool Null { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            if (!base.Equals(o)) return false;

            var that = (MetaColumn) o;

            if (Null != that.Null) return false;
            if (ColumnType != that.ColumnType) return false;
            if ((ColumnType == ColumnType.Varchar || ColumnType == ColumnType.Char)
                && !Size.Equals(that.Size)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}