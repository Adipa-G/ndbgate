using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using DbGate;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using Microsoft.Data.SqlClient;
using PerformanceTest.NDbGate.Entities.Order;
using PerformanceTest.NDbGate.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceTest.NDbGate
{
    [SimpleJob(RunStrategy.Monitoring, iterationCount: 5, warmupCount: 1)]
    [MemoryDiagnoser]
    public class NDbGateQueryBenchmark
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
        }

        [Benchmark]
        public IList<IEntity> Query()
        {
            var result = new List<IEntity>();
            var tx = _transactionFactory.CreateTransaction();
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemType = item.GetType();

                if (item is Transaction trns)
                {
                    var loaded = new SelectionQuery()
                        .From(QueryFrom.EntityType(itemType))
                        .Select(QuerySelection.EntityType(itemType))
                        .Where(QueryCondition.Expression(ConditionExpr.Build().Field(itemType, "TransactionId").Eq().Value(trns.TransactionId)))
                        .ToList(tx).FirstOrDefault();
                    if (loaded != null) result.Add((IEntity)loaded);
                }
                else if (item is Service svc)
                {
                    var loaded = new SelectionQuery()
                        .From(QueryFrom.EntityType(itemType))
                        .Select(QuerySelection.EntityType(itemType))
                        .Where(QueryCondition.Expression(ConditionExpr.Build().Field(itemType, "ItemId").Eq().Value(svc.ItemId)))
                        .ToList(tx).FirstOrDefault();
                    if (loaded != null) result.Add((IEntity)loaded);
                }
                else if (item is Product prod)
                {
                    var loaded = new SelectionQuery()
                        .From(QueryFrom.EntityType(itemType))
                        .Select(QuerySelection.EntityType(itemType))
                        .Where(QueryCondition.Expression(ConditionExpr.Build().Field(itemType, "ItemId").Eq().Value(prod.ItemId)))
                        .ToList(tx).FirstOrDefault();
                    if (loaded != null) result.Add((IEntity)loaded);
                }

                if (i % 100 == 0)
                {
                    tx.Close();
                    tx = _transactionFactory.CreateTransaction();
                }
            }
            tx.Close();
            return result;
        }

        [GlobalCleanup]
        public void GlobalCleanup() => NDbGateInsertBenchmark.TruncateNDbGateTables();
    }
}
