namespace dbgate.ermanagement.support.persistant.featureintegration.product
{
    [DbTableInfo("product_product")]
    public class Product : Item
    {
        [DbColumnInfo(DbColumnType.Double)]
        public double UnitPrice { get; set; }
    
        [DbColumnInfo(DbColumnType.Double,Nullable = true)]
        public double? BulkUnitPrice { get; set; }
    }
}
