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
    public class EfQueryBenchmark
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
            EfInsertBenchmark.TruncateEfTables();
            _items = _factory.Generate(0, EntityCount, 2);
            EfInsertBenchmark.InsertItems(_items);
        }

        [Benchmark]
        public IList<object> Query()
        {
            var result = new List<object>();
            var ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item is Transaction tx)
                {
                    var loaded = ctx.Set<Transaction>()
                        .Include(t => t.ItemTransactions)
                        .ThenInclude(it => it.ItemTransactionCharges)
                        .Single(t => t.TransactionId == tx.TransactionId);
                    result.Add(loaded);
                }
                else if (item is Service svc)
                {
                    result.Add(ctx.Set<Service>().Single(t => t.ItemId == svc.ItemId));
                }
                else if (item is Product prod)
                {
                    result.Add(ctx.Set<Product>().Single(t => t.ItemId == prod.ItemId));
                }

                if (i % 100 == 0)
                {
                    ctx.Dispose();
                    ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
                }
            }
            ctx.Dispose();
            return result;
        }

        [GlobalCleanup]
        public void GlobalCleanup() => EfInsertBenchmark.TruncateEfTables();
    }
}
