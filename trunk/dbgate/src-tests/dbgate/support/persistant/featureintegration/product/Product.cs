using dbgate.ermanagement;

namespace dbgate.support.persistant.featureintegration.product
{
    [TableInfo("product_product")]
    public class Product : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double UnitPrice { get; set; }
    
        [ColumnInfo(ColumnType.Double,Nullable = true)]
        public double? BulkUnitPrice { get; set; }
    }
}
