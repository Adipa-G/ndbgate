using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using DbGate;
using DbGateTestApp.DocGenerate;
using log4net;
using log4net.Core;

namespace DbGateTestApp
{
    [WikiCodeBlock("example_base")]
    public class ExampleBase
    {
        private static DefaultTransactionFactory transactionFactory;

        public static ITransaction SetupDb()
        {
            try
            {
                if (transactionFactory == null)
                {
                    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                    LogManager.GetLogger(typeof(ExampleBase)).Info("Starting in-memory database for unit tests");
                    transactionFactory = new DefaultTransactionFactory(
                        () => new SQLiteConnection(
                            "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON"),
                        DefaultTransactionFactory.DbSqllite);
                }
                return transactionFactory.CreateTransaction();
            }
            catch (Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(ExampleBase)).Log(typeof(ExampleBase), Level.Fatal, "Exception during database startup.", ex);
                return null;
            }
        }

        
        public static void CloseDb()
        {
            try
            {
                var connection = transactionFactory.CreateTransaction();
                connection.Close();

                transactionFactory = null;
            }
            catch (Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(ExampleBase)).Log(typeof(ExampleBase), Level.Fatal, "Exception during database cleanup.", ex);
            }
        }
    }
}
