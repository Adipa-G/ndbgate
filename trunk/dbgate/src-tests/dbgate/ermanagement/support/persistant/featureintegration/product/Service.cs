namespace dbgate.ermanagement.support.persistant.featureintegration.product
{
    [DbTableInfo("product_service")]
    public class Service : Item
    {
        [DbColumnInfo(DbColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}
