using System;
using System.Data;
using log4net;
using NUnit.Framework;

namespace dbgate.dbutility
{
    [TestFixture]
    public class DbUtilsTests
    {
        #region Setup/Teardown
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                LogManager.GetLogger(typeof (DbUtilsTests)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector(
                    "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1", DbConnector.DbSqllite);

                IDbConnection connection = dbConnector.Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE ROOT_ENTITY (ID INT PRIMARY KEY,NAME VARCHAR(12))";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during database startup.", ex);
            }
        }

        [TestFixtureTearDown]
        public static void After()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ROOT_ENTITY";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ROOT_ENTITY VALUES (10,'TEN'),(20,'TWENTY'),(30,'THIRTY')";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbUtilsTests)).Fatal("Exception during test initialization.", ex);
            }
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ROOT_ENTITY";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbUtilsTests)).Fatal("Exception during test cleanup.", ex);
            }
        }
        #endregion

        [Test]
        public void DbConnector_getConnection_databaseInitialized_shouldCreateConnection()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                Assert.IsTrue(connection.State != ConnectionState.Closed);
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbUtilsTests)).Fatal("Exception during test", ex);
                Assert.Fail(ex.Message);
            }
        }
    }
}