using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace PerformanceTest.EF.Entities.Order
{
    public class ItemTransactionTypeConfiguration : IEntityTypeConfiguration<ItemTransaction>
    {
        public void Configure(EntityTypeBuilder<ItemTransaction> builder)
        {
            builder.ToTable("EF_ItemTransaction");
            builder.HasKey(i => new { i.TransactionId, i.IndexNo });
            builder.HasMany(i => i.ItemTransactionCharges)
                .WithOne(i => i.ItemTransaction).IsRequired()
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.HasOne(i => i.Item).WithMany().IsRequired(true);
        }
    }
}
