namespace dbgate.ermanagement.support.persistant.featureintegration.product
{
    [TableInfo("product_item")]
    public abstract class Item : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int ItemId { get; set; }
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        protected Item()
        {
        }
    }
}
