using System.Data.Entity.ModelConfiguration;

namespace PerformanceTest.EF.Entities.Product
{
    public class ProductMap
    {
        public static void Map(EntityTypeConfiguration<Product> config)
        {
            config.ToTable("EF_Product");
            config.HasKey(r => r.ItemId);
        }
    }
}
