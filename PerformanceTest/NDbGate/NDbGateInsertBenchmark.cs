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
    public class NDbGateInsertBenchmark
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
            TruncateNDbGateTables();
            _items = _factory.Generate(0, EntityCount, 2);
        }

        [Benchmark]
        public void Insert()
        {
            var tx = _transactionFactory.CreateTransaction();
            for (var i = 0; i < _items.Count; i++)
            {
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
        public void GlobalCleanup() => TruncateNDbGateTables();

        internal static void TruncateNDbGateTables()
        {
            using var conn = new SqlConnection(BenchmarkConfig.NDbGateConnectionString);
            conn.Open();
            var tables = new[]
            {
                "dbgate_order_item_transaction_charge",
                "dbgate_order_item_transaction",
                "dbgate_order_transaction",
                "dbgate_product_product",
                "dbgate_product_service",
                "dbgate_product_item"
            };
            foreach (var table in tables)
            {
                using var cmd = new SqlCommand($"DELETE FROM {table}", conn);
                cmd.ExecuteNonQuery();
            }
        }

        internal static void InsertItems(IList<IEntity> items, DefaultTransactionFactory transactionFactory)
        {
            var tx = transactionFactory.CreateTransaction();
            for (var i = 0; i < items.Count; i++)
            {
                items[i].Persist(tx);
                if (i % 100 == 0)
                {
                    tx.Commit();
                    tx.Close();
                    tx = transactionFactory.CreateTransaction();
                }
            }
            tx.Commit();
            tx.Close();
        }
    }
}
