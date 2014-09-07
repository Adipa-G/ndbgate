using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.ComplexExample.Entities.Product
{
    [WikiCodeBlock("complex_example_product_product")]
    [TableInfo("product_product")]
    public class Product : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double UnitPrice { get; set; }

        [ColumnInfo(ColumnType.Double, Nullable = true)]
        public double? BulkUnitPrice { get; set; }
    }
}