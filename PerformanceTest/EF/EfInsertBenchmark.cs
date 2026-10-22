using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using PerformanceTest.EF.Entities.Order;
using PerformanceTest.EF.Entities.Product;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceTest.EF
{
    [SimpleJob(RunStrategy.Monitoring, iterationCount: 5, warmupCount: 1)]
    [MemoryDiagnoser]
    public class EfInsertBenchmark
    {
        [Params(100, 1000, 5000)]
        public int EntityCount;

        private Factory _factory = null!;
        private IList<object> _items = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _factory = new Factory();
            BenchmarkConfig.EnsureDbExists(BenchmarkConfig.EfDb);
            using var ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            ctx.Database.EnsureCreated();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            TruncateEfTables();
            _items = _factory.Generate(0, EntityCount, 2);
        }

        [Benchmark]
        public void Insert()
        {
            var ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item is Product p) ctx.Products.Add(p);
                else if (item is Service s) ctx.Services.Add(s);

                if (i % 100 == 0 || (i < _items.Count - 1 && _items[i].GetType() != _items[i + 1].GetType()))
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();

            ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is Transaction tx)
                {
                    foreach (var it in tx.ItemTransactions)
                        ctx.Items.Attach(it.Item);
                    ctx.Transactions.Add(tx);
                }
                if (i % 100 == 0 || (i < _items.Count - 1 && _items[i].GetType() != _items[i + 1].GetType()))
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();
        }

        [GlobalCleanup]
        public void GlobalCleanup() => TruncateEfTables();

        internal static void TruncateEfTables()
        {
            using var ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            ctx.Database.ExecuteSqlRaw("DELETE FROM EF_ItemTransactionCharge");
            ctx.Database.ExecuteSqlRaw("DELETE FROM EF_ItemTransaction");
            ctx.Database.ExecuteSqlRaw("DELETE FROM EF_Transaction");
            ctx.Database.ExecuteSqlRaw("DELETE FROM EF_Product");
            ctx.Database.ExecuteSqlRaw("DELETE FROM EF_Service");
            ctx.Database.ExecuteSqlRaw("DELETE FROM EF_Item");
        }

        internal static void InsertItems(IList<object> items)
        {
            var ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] is Product p) ctx.Products.Add(p);
                else if (items[i] is Service s) ctx.Services.Add(s);

                if (i % 100 == 0 || (i < items.Count - 1 && items[i].GetType() != items[i + 1].GetType()))
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();

            ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] is Transaction tx)
                {
                    foreach (var it in tx.ItemTransactions)
                        ctx.Items.Attach(it.Item);
                    ctx.Transactions.Add(tx);
                }
                if (i % 100 == 0 || (i < items.Count - 1 && items[i].GetType() != items[i + 1].GetType()))
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();
        }
    }
}
