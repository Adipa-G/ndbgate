using System.Data.Entity.ModelConfiguration;

namespace PerformanceTest.EF.Entities.Order
{
    public class ItemTransactionMap
    {
        public static void Map(EntityTypeConfiguration<ItemTransaction> config)
        {
            config.ToTable("EF_ItemTransaction");
            config.HasKey(r => new { r.TransactionId,r.IndexNo});
            config.HasMany(r => r.ItemTransactionCharges)
                .WithRequired(r => r.ItemTransaction)
                .WillCascadeOnDelete(false);
            config.HasOptional(r => r.Item);
        }
    }
}
