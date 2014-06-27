namespace DbGate.Support.Persistant.FeatureIntegration.Product
{
    [TableInfo("product_service")]
    public class Service : Item
    {
        [ColumnInfo(ColumnType.Double)]
        public double HourlyRate { get; set; }
    }
}