namespace dbgate.ermanagement.support.persistant.featureintegration.product
{
    [DbTableInfo("product_item")]
    public abstract class Item : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int ItemId { get; set; }
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        protected Item()
        {
        }
    }
}
