using DbGate;

namespace DbGateTestApp.ComplexExample.Entities.Product
{
    [TableInfo("product_service")]
    public class Service : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}