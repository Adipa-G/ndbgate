namespace dbgate.ermanagement.support.persistant.featureintegration.product
{
    [TableInfo("product_service")]
    public class Service : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}
