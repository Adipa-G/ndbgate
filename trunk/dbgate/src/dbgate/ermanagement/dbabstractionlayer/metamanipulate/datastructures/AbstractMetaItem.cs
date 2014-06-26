using System;

namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.datastructures
{
    public abstract class AbstractMetaItem : IMetaItem
    {
        private string _name;
        private MetaItemType _itemType;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public MetaItemType ItemType
        {
            get { return _itemType; }
            set { _itemType = value; }
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (!(o is AbstractMetaItem)) return false;

            AbstractMetaItem that = (AbstractMetaItem) o;

            if (_itemType != that._itemType) return false;
            if (!_name.Equals(that._name,StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
