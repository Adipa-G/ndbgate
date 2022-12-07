using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare
{
    public abstract class AbstractMetaComparisonGroup : IMetaComparisonGroup
    {
        private IMetaItem existingItem;
        private IMetaItem requiredItem;

        protected AbstractMetaComparisonGroup(IMetaItem existingItem, IMetaItem requiredItem)
        {
            this.existingItem = existingItem;
            this.requiredItem = requiredItem;
        }

        #region IMetaComparisonGroup Members

        public abstract bool ShouldCreateInDb();

        public abstract bool ShouldDeleteFromDb();

        public abstract bool ShouldAlterInDb();

        public MetaItemType ItemType
        {
            get
            {
                if (existingItem != null)
                {
                    return existingItem.ItemType;
                }
                else if (requiredItem != null)
                {
                    return requiredItem.ItemType;
                }
                return MetaItemType.Unknown;
            }
        }

        public virtual IMetaItem ExistingItem
        {
            get => existingItem;
            set => existingItem = value;
        }

        public IMetaItem RequiredItem
        {
            get => requiredItem;
            set => requiredItem = value;
        }

        #endregion
    }
}