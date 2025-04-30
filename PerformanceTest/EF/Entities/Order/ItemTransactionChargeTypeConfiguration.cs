using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PerformanceTest.EF.Entities.Order
{
    public class ItemTransactionChargeTypeConfiguration : IEntityTypeConfiguration<ItemTransactionCharge>
    {
        public void Configure(EntityTypeBuilder<ItemTransactionCharge> builder)
        {
            builder.ToTable("EF_ItemTransactionCharge");
            builder.HasKey(r => new { r.TransactionId, r.IndexNo, r.ChargeIndex });
        }
    }
}