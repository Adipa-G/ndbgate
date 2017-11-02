using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using DbGate;
using log4net;
using log4net.Core;
using PerformanceTest.NDbGate.Entities.Order;
using PerformanceTest.NDbGate.Entities.Product;

namespace PerformanceTest.NDbGate
{
    public class NDbGatePerformanceCounter
    {
        private int _perThread = 100;
        private DefaultTransactionFactory _transactionFactory;
        
        public NDbGatePerformanceCounter()
        {
            try
            {
                if (_transactionFactory == null)
                {
                    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                    LogManager.GetLogger(typeof(NDbGatePerformanceCounter)).Info("Connecting to sql server for performance testing");
                    _transactionFactory = new DefaultTransactionFactory(() => new SqlConnection(
                        "Data Source=localhost;Integrated Security=SSPI;Initial Catalog=DbGate"),
                        DefaultTransactionFactory.DbSqlServer);

                    var tx = _transactionFactory.CreateTransaction();

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
            var items = new Factory().Generate(seed,_perThread, 10);
            var tx = _transactionFactory.CreateTransaction();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Persist(tx);
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
    }
}
