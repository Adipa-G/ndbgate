using System.Data.Entity.ModelConfiguration;

namespace PerformanceTest.EF.Entities.Order
{
    public class TransactionMap
    {
        public static void Map(EntityTypeConfiguration<Transaction> config)
        {
            config.ToTable("EF_Transaction");
            config.HasKey(r => r.TransactionId);
            config.HasMany(r => r.ItemTransactions)
                .WithRequired(r => r.Transaction)
                .WillCascadeOnDelete(false);
        }
    }
}
