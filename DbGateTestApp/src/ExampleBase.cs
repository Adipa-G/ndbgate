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
        public static IDbConnection SetupDb()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(),typeof(ExampleBase)).Log(typeof(ExampleBase),Level.Info,"Starting in-memory database for unit tests",null);
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
                return dbConnector.Connection;
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
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(ExampleBase)).Log(typeof(ExampleBase), Level.Fatal, "Exception during database cleanup.", ex);
            }
        }
    }
}
