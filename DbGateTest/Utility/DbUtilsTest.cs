using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Core;
using Xunit;

namespace DbGate.Utility
{
    [Collection("Sequential")]
    public class DbUtilsTests
    {
        private static ITransactionFactory transactionFactory;

        private static ITransaction SetupDb()
        {
            try
            {
                if (transactionFactory == null)
                {
                    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                    LogManager.GetLogger(typeof(DbUtilsTests)).Info("Starting in-memory database for unit tests");
                    transactionFactory = new DefaultTransactionFactory(
                        () => new SQLiteConnection(
                            "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON"),
                        DefaultTransactionFactory.DbSqllite);
                }
                return transactionFactory.CreateTransaction();
            }
            catch (System.Exception ex)
            {
                LoggerManager.GetLogger(Assembly.GetExecutingAssembly(), typeof(DbUtilsTests)).Log(typeof(DbUtilsTests),
                    Level.Fatal, "Exception during database startup.", ex);
                return null;
            }
        }

        [Fact]
        public void Utils_GetConnection_DatabaseInitialized_ShouldCreateConnection()
        {
            try
            {
                var transaction = SetupDb();
                var connection = transaction.Connection;
                Assert.True(connection.State != ConnectionState.Closed);
                transaction.Close();
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during test", ex);
                Assert.Fail(ex.Message);
            }
        }
    }
}