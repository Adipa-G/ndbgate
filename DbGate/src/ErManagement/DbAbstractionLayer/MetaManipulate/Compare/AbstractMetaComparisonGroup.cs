using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare
{
    public abstract class AbstractMetaComparisonGroup : IMetaComparisonGroup
    {
        private IMetaItem _existingItem;
        private IMetaItem _requiredItem;

        protected AbstractMetaComparisonGroup(IMetaItem existingItem, IMetaItem requiredItem)
        {
            _existingItem = existingItem;
            _requiredItem = requiredItem;
        }

        #region IMetaComparisonGroup Members

        public abstract bool ShouldCreateInDb();

        public abstract bool ShouldDeleteFromDb();

        public abstract bool ShouldAlterInDb();

        public MetaItemType ItemType
        {
            get
            {
                if (_existingItem != null)
                {
                    return _existingItem.ItemType;
                }
                else if (_requiredItem != null)
                {
                    return _requiredItem.ItemType;
                }
                return MetaItemType.Unknown;
            }
        }

        public virtual IMetaItem ExistingItem
        {
            get { return _existingItem; }
            set { _existingItem = value; }
        }

        public IMetaItem RequiredItem
        {
            get { return _requiredItem; }
            set { _requiredItem = value; }
        }

        #endregion
    }
}