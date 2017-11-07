using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using PerformanceTest.EF.Entities.Order;
using PerformanceTest.EF.Entities.Product;

namespace PerformanceTest.EF
{
    public class TestDbContext : DbContext
    {
//        private static bool isDbInterceptionInitialised = false;

        public TestDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
//            if (!isDbInterceptionInitialised)
//            {
//                DbInterception.Add(new InsertUpdateInterceptor());
//                isDbInterceptionInitialised = true;
//            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ProductMap.Map(modelBuilder.Entity<Product>());
            ServiceMap.Map(modelBuilder.Entity<Service>());

            TransactionMap.Map(modelBuilder.Entity<Transaction>());
            ItemTransactionMap.Map(modelBuilder.Entity<ItemTransaction>());
            ItemTransactionChargeMap.Map(modelBuilder.Entity<ItemTransactionCharge>());
        }
    }
}
