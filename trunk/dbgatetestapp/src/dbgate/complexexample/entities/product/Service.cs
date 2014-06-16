using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.complexexample.entities.product
{
    [TableInfo("product_service")]
    public class Service : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}
