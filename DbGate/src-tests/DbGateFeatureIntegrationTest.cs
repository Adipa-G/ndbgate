using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DbGate.Support.Persistant.FeatureIntegration.Order;
using DbGate.Support.Persistant.FeatureIntegration.Product;
using DbGate.Utility;
using NUnit.Framework;
using log4net;
using log4net.Config;

namespace DbGate
{
    public class DbGateFeatureIntegrationTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateFeatureIntegrationTest)).Info(
                    "Starting in-memory database for unit tests");
                var dbConnector =
                    new DbConnector(
                        "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON",
                        DbConnector.DbSqllite);
                Assert.IsNotNull(dbConnector.Connection);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateFeatureIntegrationTest)).Fatal("Exception during database startup.",
                                                                                  ex);
            }
        }

        [TestFixtureTearDown]
        public static void After()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateFeatureIntegrationTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().ClearCache();
            }
        }

        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            ICollection<Type> types = new List<Type>();
            types.Add(typeof (ItemTransaction));
            types.Add(typeof (ItemTransactionCharge));
            types.Add(typeof (Transaction));
            types.Add(typeof (Product));
            types.Add(typeof (Service));
            ErManagement.ErMapper.DbGate.GetSharedInstance().PatchDataBase(connection, types, true);

            transaction.Commit();
            return connection;
        }

        [Test]
        public void FeatureIntegration_PersistAndRetrieve_WithComplexStructure_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                int transId = 35;
                int productId = 135;
                int serviceId = 235;

                IDbConnection connection = SetupTables();
                Product product = CreateDefaultProduct(connection, productId);
                Service service = CreateDefaultService(connection, serviceId);
                Transaction transaction = CreateDefaultTransaction(connection, transId, product, service);

                var loadedTransaction = new Transaction();
                LoadWithId(connection, loadedTransaction, transId);
                DbMgtUtility.Close(connection);

                VerifyEquals(transaction, loadedTransaction);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private void VerifyEquals(Transaction transaction, Transaction loadedTransaction)
        {
            Assert.AreEqual(loadedTransaction.Name, transaction.Name);
            foreach (ItemTransaction orgItemTransaction in transaction.ItemTransactions)
            {
                bool foundItem = false;
                foreach (ItemTransaction loadedItemTransaction in loadedTransaction.ItemTransactions)
                {
                    if (orgItemTransaction.IndexNo == loadedItemTransaction.IndexNo)
                    {
                        foundItem = true;
                        Assert.AreEqual(orgItemTransaction.Item.Name, loadedItemTransaction.Item.Name);
                        Assert.AreEqual(orgItemTransaction.Item.ItemId, loadedItemTransaction.Item.ItemId);
                        Assert.AreSame(loadedItemTransaction.Transaction, loadedTransaction);

                        foreach (ItemTransactionCharge orgTransactionCharge in orgItemTransaction.ItemTransactionCharges
                            )
                        {
                            bool foundCharge = false;
                            foreach (
                                ItemTransactionCharge loadedTransactionCharge in
                                    loadedItemTransaction.ItemTransactionCharges)
                            {
                                if (orgTransactionCharge.IndexNo == loadedTransactionCharge.IndexNo
                                    && orgTransactionCharge.ChargeIndex == loadedTransactionCharge.ChargeIndex)
                                {
                                    foundCharge = true;
                                    Assert.AreEqual(orgTransactionCharge.ChargeCode, loadedTransactionCharge.ChargeCode);
                                    Assert.AreSame(loadedTransactionCharge.Transaction, loadedTransaction);
                                    Assert.AreSame(loadedTransactionCharge.ItemTransaction, loadedItemTransaction);
                                }
                            }
                            Assert.IsTrue(foundCharge, "Item transaction charge not found");
                        }
                    }
                }
                Assert.IsTrue(foundItem, "Item transaction not found");
            }
            Assert.AreEqual(loadedTransaction.Name, transaction.Name);
        }

        private bool LoadWithId(IDbConnection connection, Transaction loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from order_transaction where transaction_id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, connection);
                loaded = true;
            }

            return loaded;
        }

        public Product CreateDefaultProduct(IDbConnection con, int productId)
        {
            var product = new Product();
            product.ItemId = productId;
            product.Name = "Product";
            product.UnitPrice = 54;
            IDbTransaction transaction = con.BeginTransaction();
            product.Persist(con);
            transaction.Commit();
            return product;
        }

        public Service CreateDefaultService(IDbConnection con, int serviceId)
        {
            var service = new Service();
            service.ItemId = serviceId;
            service.Name = "Service";
            service.HourlyRate = 65;
            IDbTransaction transaction = con.BeginTransaction();
            service.Persist(con);
            transaction.Commit();
            return service;
        }

        public Transaction CreateDefaultTransaction(IDbConnection con, int transactionId, Product product,
                                                    Service service)
        {
            var transaction = new Transaction();
            transaction.TransactionId = transactionId;
            transaction.Name = "TRS-0001";

            var productTransaction = new ItemTransaction(transaction);
            productTransaction.IndexNo = 0;
            productTransaction.Item = product;
            transaction.ItemTransactions.Add(productTransaction);

            var productTransactionCharge = new ItemTransactionCharge(productTransaction);
            productTransactionCharge.ChargeCode = "Product-Sell-Code";
            productTransaction.ItemTransactionCharges.Add(productTransactionCharge);

            var serviceTransaction = new ItemTransaction(transaction);
            serviceTransaction.IndexNo = 1;
            serviceTransaction.Item = service;
            transaction.ItemTransactions.Add(serviceTransaction);

            var serviceTransactionCharge = new ItemTransactionCharge(serviceTransaction);
            serviceTransactionCharge.ChargeCode = "Service-Sell-Code";
            serviceTransaction.ItemTransactionCharges.Add(serviceTransactionCharge);

            IDbTransaction dbTransaction = con.BeginTransaction();
            transaction.Persist(con);
            dbTransaction.Commit();
            return transaction;
        }
    }
}