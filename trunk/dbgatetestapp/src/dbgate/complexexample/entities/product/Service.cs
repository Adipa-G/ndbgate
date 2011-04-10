using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.complexexample.entities.product
{
    [DbTableInfo("product_service")]
    public class Service : Item
    {
        [DbColumnInfo(DbColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}
