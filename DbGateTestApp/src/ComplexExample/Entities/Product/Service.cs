using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.ComplexExample.Entities.Product
{
    [WikiCodeBlock("complex_example_product_service")]
    [TableInfo("product_service")]
    public class Service : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}