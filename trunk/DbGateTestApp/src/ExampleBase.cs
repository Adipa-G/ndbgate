using System;
using System.Data;
using System.IO;
using System.Reflection;
using DbGate;
using log4net.Core;

namespace DbGateTestApp
{
    public class ExampleBase
    {
        private static DefaultTransactionFactory _transactionFactory;

        public static ITransaction SetupDb()
        {
            try
            {
                if (_transactionFactory == null)
                {
                    log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));


                    LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(ExampleBase)).Log(typeof(ExampleBase), Level.Info, "Starting in-memory database for unit tests", null);
                    _transactionFactory = new DefaultTransactionFactory("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DefaultTransactionFactory.DbSqllite);
                }
                return _transactionFactory.CreateTransaction();
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
                ITransaction connection = _transactionFactory.CreateTransaction();
                connection.Close();

                _transactionFactory = null;
            }
            catch (Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(ExampleBase)).Log(typeof(ExampleBase), Level.Fatal, "Exception during database cleanup.", ex);
            }
        }
    }
}
