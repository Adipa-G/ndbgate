using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PerformanceTest.EF.Entities.Order;
using PerformanceTest.EF.Entities.Product;

namespace PerformanceTest.EF
{
    public class TestDbContext : DbContext
    {
        private readonly string connectionString;

        public TestDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
                optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new ItemTypeConfiguration().Configure(modelBuilder.Entity<Item>());
            new ProductTypeConfiguration().Configure(modelBuilder.Entity<Product>());
            new ServiceTypeConfiguration().Configure(modelBuilder.Entity<Service>());
            new TransactionTypeConfiguration().Configure(modelBuilder.Entity<Transaction>());
            new ItemTransactionTypeConfiguration().Configure(modelBuilder.Entity<ItemTransaction>());
            new ItemTransactionChargeTypeConfiguration().Configure(modelBuilder.Entity<ItemTransactionCharge>());
        }

        public DbSet<Item> Items { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        
        public DbSet<ItemTransaction> ItemTransactions { get; set; }
        
        public DbSet<ItemTransactionCharge> ItemTransactionCharges { get; set; }
    }
}
