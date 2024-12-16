using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PerformanceTest.EF.Entities.Product
{
    public class ItemTypeConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("EF_Item");
            builder.HasKey(r => r.ItemId);
            builder.UseTptMappingStrategy();
        }
    }
}
