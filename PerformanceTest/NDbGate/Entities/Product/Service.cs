using DbGate;

namespace PerformanceTest.NDbGate.Entities.Product
{
    [TableInfo("dbgate_product_service")]
    public class Service : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}