using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DbGate;
using DbGate.ErManagement.Query;
using log4net;
using log4net.Core;
using PerformanceTest.NDbGate.Entities.Order;
using PerformanceTest.NDbGate.Entities.Product;

namespace PerformanceTest.NDbGate
{
    public class NDbGatePerformanceCounter
    {
        private readonly int perThread;
        private readonly DefaultTransactionFactory transactionFactory;

        public NDbGatePerformanceCounter(string connectionString,int perThread)
        {
            this.perThread = perThread;

            try
            {
                if (transactionFactory == null)
                {
                    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                    LogManager.GetLogger(typeof(NDbGatePerformanceCounter)).Info("Connecting to sql server for performance testing");
                    transactionFactory = new DefaultTransactionFactory(() => new SqlConnection(connectionString),
                        DefaultTransactionFactory.DbSqlServer);

                    var tx = transactionFactory.CreateTransaction();

                    ICollection<Type> entityTypes = new List<Type>();
                    entityTypes.Add(typeof(Product));
                    entityTypes.Add(typeof(Service));
                    entityTypes.Add(typeof(Transaction));
                    entityTypes.Add(typeof(ItemTransaction));
                    entityTypes.Add(typeof(ItemTransactionCharge));

                    tx.DbGate.PatchDataBase(tx, entityTypes, true);
                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(NDbGatePerformanceCounter)).Log(
                    typeof(NDbGatePerformanceCounter), Level.Fatal, "Exception during database startup.", ex);
            }
        }

        public void Start(int threads)
        {
            var threadList = new List<Thread>();
            for (int i = 0; i < threads; i++)
            {
                var copy = i;
                var thread = new Thread(() => DoInThread(copy * 100000));
                thread.Start();

                threadList.Add(thread);
            }

            foreach (var thread in threadList)
            {
                thread.Join();
            }
        }

        private void DoInThread(int seed)
        {
            var factory = new Factory();

            var items = factory.Generate(seed, perThread, 10);
            InsertTest(items);
            //QueryTest(items);

            factory.Update(items);
            UpdateTest(items);
            DeleteTest(items);
        }

        private void InsertTest(IList<IEntity> items)
        {
            var sw = new Stopwatch();
            
            sw.Start();
            var tx = transactionFactory.CreateTransaction();
            for (int i = 0; i < items.Count; i++)
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
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(NDbGatePerformanceCounter))
                .Log(typeof(NDbGatePerformanceCounter), Level.Warn, $"NDBGate Thread Insert speed  {speed} entities/second", null);
        }

        private void UpdateTest(IList<IEntity> items)
        {
            var sw = new Stopwatch();

            sw.Start();
            var tx = transactionFactory.CreateTransaction();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Status = EntityStatus.Modified;
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
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(NDbGatePerformanceCounter))
                .Log(typeof(NDbGatePerformanceCounter), Level.Warn, $"NDBGate Thread Update speed  {speed} entities/second", null);
        }

        private void QueryTest(IList<IEntity> items)
        {
            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                var tx = transactionFactory.CreateTransaction();
                var loaded = new SelectionQuery()
                    .From(QueryFrom.EntityType(item.GetType()))
                    .Select(QuerySelection.EntityType(item.GetType()))
                    .ToList(tx).FirstOrDefault();
                tx.Close();
            }
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(NDbGatePerformanceCounter))
                .Log(typeof(NDbGatePerformanceCounter), Level.Warn, $"NDBGate Thread Update speed  {speed} entities/second", null);
        }

        private void DeleteTest(IList<IEntity> items)
        {
            var sw = new Stopwatch();

            sw.Start();
            var tx = transactionFactory.CreateTransaction();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Status = EntityStatus.Deleted;
                items[i].Persist(tx);
                if (i % 100 == 0 || (i > 0 && items[i].GetType() != items[i -1].GetType()))
                {
                    tx.Commit();
                    tx.Close();
                    tx = transactionFactory.CreateTransaction();
                }
            }
            tx.Commit();
            tx.Close();
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(NDbGatePerformanceCounter))
                .Log(typeof(NDbGatePerformanceCounter), Level.Warn, $"NDBGate Thread Delete speed  {speed} entities/second", null);
        }
    }
}
