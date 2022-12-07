using System;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures
{
    public abstract class AbstractMetaItem : IMetaItem
    {
        private MetaItemType itemType;
        private string name;

        #region IMetaItem Members

        public string Name
        {
            get => name;
            set => name = value;
        }

        public MetaItemType ItemType
        {
            get => itemType;
            set => itemType = value;
        }

        #endregion

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (!(o is AbstractMetaItem)) return false;

            var that = (AbstractMetaItem) o;

            if (itemType != that.itemType) return false;
            if (!name.Equals(that.name, StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}