using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace PerformanceTest.EF
{
    [SimpleJob(RunStrategy.Monitoring, iterationCount: 5, warmupCount: 1)]
    [MemoryDiagnoser]
    public class EfDeleteBenchmark
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
        public void Delete()
        {
            var ctx = new TestDbContext(BenchmarkConfig.EfConnectionString);
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                ctx.Attach(_items[i]);
                ctx.Remove(_items[i]);

                if (i % 100 == 0)
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
        public void GlobalCleanup() => EfInsertBenchmark.TruncateEfTables();
    }
}
