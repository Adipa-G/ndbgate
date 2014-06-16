using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.complexexample.entities.product
{
    [TableInfo("product_product")]
    public class Product : dbgatetestapp.dbgate.complexexample.entities.product.Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double UnitPrice { get; set; }
    
        [ColumnInfo(ColumnType.Double,Nullable = true)]
        public double? BulkUnitPrice { get; set; }
    }
}
