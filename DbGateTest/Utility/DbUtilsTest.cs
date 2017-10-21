using System.Data;
using System.Reflection;
using log4net;
using log4net.Config;
using NUnit.Framework;

namespace DbGate.Utility
{
    [TestFixture]
    public class DbUtilsTests
    {
        #region Setup/Teardown
        private static ITransactionFactory _transactionFactory;

        [OneTimeSetUp]
        public static void Before()
        {
            try
            {
                var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(repository);

                LogManager.GetLogger(typeof(DbUtilsTests)).Info("Starting in-memory database for unit tests");
                _transactionFactory = new DefaultTransactionFactory(
                    "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1", DefaultTransactionFactory.DbSqllite);

                var transaction = _transactionFactory.CreateTransaction();

                IDbCommand command = transaction.CreateCommand();
                command.CommandText = "CREATE TABLE ROOT_ENTITY (ID INT PRIMARY KEY,NAME VARCHAR(12))";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(typeof(DbUtilsTests)).Fatal("Exception during database startup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            try
            {
                var transaction = _transactionFactory.CreateTransaction();

                IDbCommand command = transaction.CreateCommand();
                command.CommandText = "INSERT INTO ROOT_ENTITY VALUES (10,'TEN'),(20,'TWENTY'),(30,'THIRTY')";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during test initialization.", ex);
            }
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                var transaction = _transactionFactory.CreateTransaction();

                IDbCommand command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM ROOT_ENTITY";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        #endregion

        

        [OneTimeTearDown]
        public static void After()
        {
            try
            {
                var transaction = _transactionFactory.CreateTransaction();

                IDbCommand command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM ROOT_ENTITY";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [Test]
        public void Utils_GetConnection_DatabaseInitialized_ShouldCreateConnection()
        {
            try
            {
                var transaction = _transactionFactory.CreateTransaction();
                IDbConnection connection = transaction.Connection;
                Assert.IsTrue(connection.State != ConnectionState.Closed);
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