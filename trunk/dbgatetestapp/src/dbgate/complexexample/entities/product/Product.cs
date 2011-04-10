using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.complexexample.entities.product
{
    [DbTableInfo("product_product")]
    public class Product : dbgatetestapp.dbgate.complexexample.entities.product.Item
    {
        [DbColumnInfo(DbColumnType.Double)]
        public double UnitPrice { get; set; }
    
        [DbColumnInfo(DbColumnType.Double,Nullable = true)]
        public double? BulkUnitPrice { get; set; }
    }
}
