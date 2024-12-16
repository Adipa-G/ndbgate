using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace PerformanceTest.EF.Entities.Order
{
    public class TransactionTypeConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("EF_Transaction");
            builder.HasKey(r => r.TransactionId);
            builder.HasMany(r => r.ItemTransactions)
                .WithOne(r => r.Transaction).IsRequired()
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
