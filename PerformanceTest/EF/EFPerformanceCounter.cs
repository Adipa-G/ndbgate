using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Core;
using Microsoft.EntityFrameworkCore;
using PerformanceTest.EF.Entities.Order;
using PerformanceTest.EF.Entities.Product;

namespace PerformanceTest.EF
{
    public class EfPerformanceCounter
    {
        private readonly int perThread;
        private readonly string connectionString;

        private readonly Factory factory;

        public EfPerformanceCounter(string connectionString, int perThread)
        {
            this.perThread = perThread;
            this.connectionString = connectionString;
            this.factory = new Factory();

            try
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                TestDbContext context = new TestDbContext(connectionString);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Database.Migrate();

                LogManager.GetLogger(typeof(EfPerformanceCounter)).Info("Connecting to sql server for performance testing");
            }
            catch (Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(EfPerformanceCounter)).Log(
                    typeof(EfPerformanceCounter), Level.Fatal, "Exception during database startup.", ex);
            }
        }

        public void Start(int threads)
        {
            var threadList = new List<Thread>();
            for (var i = 0; i < threads; i++)
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
            var items = factory.Generate(seed, perThread, 2);

            InsertTest(items);

            items = QueryTest(items);

            items = UpdateTest(items);

            DeleteTest(items);
        }

        private void InsertTest(IList<object> items)
        {
            var sw = new Stopwatch();

            sw.Start();
            var ctx = new TestDbContext(connectionString);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemType = item.GetType();
               
                if (item is Product)
                {
                    ctx.Products.Add((Product)item);
                }
                else if (itemType == typeof(Service))
                {
                    ctx.Services.Add((Service)item);
                }

                if (i % 100 == 0 || (i < items.Count - 1 && items[i].GetType() != items[i + 1].GetType()))
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(connectionString);
                }
            }

            ctx.SaveChanges();
            ctx.Dispose();

            ctx = new TestDbContext(connectionString);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemType = item.GetType();
                if (itemType == typeof(Transaction))
                {
                    var tx = (Transaction)item;

                    foreach (var txItemTransaction in tx.ItemTransactions)
                    {
                        ctx.Items.Attach(txItemTransaction.Item);
                    }
                    ctx.Transactions.Add(tx);
                }

                if (i % 100 == 0 || (i < items.Count - 1 && items[i].GetType() != items[i + 1].GetType()))
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(connectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(EfPerformanceCounter))
                .Log(typeof(EfPerformanceCounter), Level.Warn, $"EF Thread Insert speed  {speed} entities/second", null);
        }

        private IList<object> QueryTest(IList<object> items)
        {
            var newList = new List<object>();
            var sw = new Stopwatch();

            sw.Start();
            var ctx = new TestDbContext(connectionString);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemType = item.GetType();

                if (itemType == typeof(Transaction))
                {
                    var tx = (Transaction)item;
                    var loaded = ctx.Set<Transaction>()
                        .Include(t => t.ItemTransactions)
                        .ThenInclude(it => it.ItemTransactionCharges)
                        .Single(t => t.TransactionId == tx.TransactionId);
                    newList.Add(loaded);
                }
                else 
                if (itemType == typeof(Service))
                {
                    var svc = (Service)item;
                    var loaded = ctx.Set<Service>().Single(t => t.ItemId == svc.ItemId);
                    newList.Add(loaded);
                }
                else if (itemType == typeof(Product))
                {
                    var product = (Product)item;
                    var loaded = ctx.Set<Product>().Single(t => t.ItemId == product.ItemId);
                    newList.Add(loaded);
                }

                if (i % 100 == 0)
                {
                    ctx.Dispose();
                    ctx = new TestDbContext(connectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(EfPerformanceCounter))
                .Log(typeof(EfPerformanceCounter), Level.Warn, $"EF Thread Query speed  {speed} entities/second", null);
            return newList;
        }

        private IList<object> UpdateTest(IList<object> items)
        {
            var newList = new List<object>();
            var sw = new Stopwatch();

            sw.Start();
            var ctx = new TestDbContext(connectionString);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var itemType = item.GetType();

                if (itemType == typeof(Transaction))
                {
                    var tx = (Transaction)item;
                    var loaded = ctx.Set<Transaction>()
                        .Include(r => r.ItemTransactions)
                        .ThenInclude(it => it.ItemTransactionCharges)
                        .Single(t => t.TransactionId == tx.TransactionId);
                    factory.Update(loaded);
                    newList.Add(loaded);
                }
                else if (itemType == typeof(Service))
                {
                    var svc = (Service)item;
                    var loaded = ctx.Set<Service>().Single(t => t.ItemId == svc.ItemId);
                    factory.Update(loaded);
                    newList.Add(loaded);
                }
                else if (itemType == typeof(Product))
                {
                    var product = (Product)item;
                    var loaded = ctx.Set<Product>().Single(t => t.ItemId == product.ItemId);
                    factory.Update(loaded);
                    newList.Add(loaded);
                }

                if (i % 100 == 0)
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(connectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(EfPerformanceCounter))
                .Log(typeof(EfPerformanceCounter), Level.Warn, $"EF Thread Update speed  {speed} entities/second", null);
            return newList;
        }

        private void DeleteTest(IList<object> items)
        {
            var sw = new Stopwatch();

            sw.Start();
            var ctx = new TestDbContext(connectionString);

            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                ctx.Attach(item);
                ctx.Remove(item);

                if (i % 100 == 0)
                {
                    ctx.SaveChanges();
                    ctx.Dispose();
                    ctx = new TestDbContext(connectionString);
                }
            }
            ctx.SaveChanges();
            ctx.Dispose();
            sw.Stop();

            var speed = items.Count * 1000 / sw.ElapsedMilliseconds;

            LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(EfPerformanceCounter))
                .Log(typeof(EfPerformanceCounter), Level.Warn, $"EF Thread Delete speed  {speed} entities/second", null);
        }
    }
}
