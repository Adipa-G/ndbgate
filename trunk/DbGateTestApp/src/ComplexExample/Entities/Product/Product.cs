using DbGate;

namespace DbGateTestApp.ComplexExample.Entities.Product
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