namespace PerformanceTest.EF.Entities.Product
{
    public class Product : Item
    {
        public double UnitPrice { get; set; }

        public double? BulkUnitPrice { get; set; }
    }
}
