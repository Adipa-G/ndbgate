using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using DbGate;
using Microsoft.Data.SqlClient;
using PerformanceTest.NDbGate.Entities.Order;
using PerformanceTest.NDbGate.Entities.Product;
using System;
using System.Collections.Generic;

namespace PerformanceTest.NDbGate
{
    [SimpleJob(RunStrategy.Monitoring, iterationCount: 5, warmupCount: 1)]
    [MemoryDiagnoser]
    public class NDbGateUpdateBenchmark
    {
        [Params(100, 1000, 5000)]
        public int EntityCount;

        private Factory _factory = null!;
        private DefaultTransactionFactory _transactionFactory = null!;
        private IList<IEntity> _items = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _factory = new Factory();
            BenchmarkConfig.EnsureDbExists(BenchmarkConfig.NDbGateDb);
            _transactionFactory = new DefaultTransactionFactory(
                () => new SqlConnection(BenchmarkConfig.NDbGateConnectionString),
                DefaultTransactionFactory.DbSqlServer);

            var tx = _transactionFactory.CreateTransaction();
            tx.DbGate.PatchDataBase(tx, new[]
            {
                typeof(Product), typeof(Service),
                typeof(Transaction), typeof(ItemTransaction), typeof(ItemTransactionCharge)
            }, true);
            tx.Commit();
            tx.Close();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            NDbGateInsertBenchmark.TruncateNDbGateTables();
            _items = _factory.Generate(0, EntityCount, 2);
            NDbGateInsertBenchmark.InsertItems(_items, _transactionFactory);
            _factory.Update(_items);
        }

        [Benchmark]
        public void Update()
        {
            var tx = _transactionFactory.CreateTransaction();
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].Status = EntityStatus.Modified;
                _items[i].Persist(tx);
                if (i % 100 == 0)
                {
                    tx.Commit();
                    tx.Close();
                    tx = _transactionFactory.CreateTransaction();
                }
            }
            tx.Commit();
            tx.Close();
        }

        [GlobalCleanup]
        public void GlobalCleanup() => NDbGateInsertBenchmark.TruncateNDbGateTables();
    }
}
