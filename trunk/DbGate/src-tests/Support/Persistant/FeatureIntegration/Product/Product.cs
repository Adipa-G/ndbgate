namespace DbGate.Support.Persistant.FeatureIntegration.Product
{
    [TableInfo("product_product")]
    public class Product : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double UnitPrice { get; set; }

        [ColumnInfo(ColumnType.Double, Nullable = true)]
        public double? BulkUnitPrice { get; set; }
    }
}