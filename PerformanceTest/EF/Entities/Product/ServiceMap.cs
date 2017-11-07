using System.Data.Entity.ModelConfiguration;

namespace PerformanceTest.EF.Entities.Product
{
    public class ServiceMap
    {
        public static void Map(EntityTypeConfiguration<Service> config)
        {
            config.ToTable("EF_Service");
            config.HasKey(r => r.ItemId);
        }
    }
}
