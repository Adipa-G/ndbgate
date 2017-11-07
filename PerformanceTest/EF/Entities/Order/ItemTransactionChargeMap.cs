using System.Data.Entity.ModelConfiguration;

namespace PerformanceTest.EF.Entities.Order
{
    public class ItemTransactionChargeMap
    {
        public static void Map(EntityTypeConfiguration<ItemTransactionCharge> config)
        {
            config.ToTable("EF_ItemTransactionCharge");
            config.HasKey(r => new {r.TransactionId,r.IndexNo,r.ChargeIndex});
        }
    }
}
