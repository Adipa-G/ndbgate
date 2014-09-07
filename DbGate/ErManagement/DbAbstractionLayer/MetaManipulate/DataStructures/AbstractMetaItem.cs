using System;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures
{
    public abstract class AbstractMetaItem : IMetaItem
    {
        private MetaItemType _itemType;
        private string _name;

        #region IMetaItem Members

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

        #endregion

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (!(o is AbstractMetaItem)) return false;

            var that = (AbstractMetaItem) o;

            if (_itemType != that._itemType) return false;
            if (!_name.Equals(that._name, StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}