using System;
using System.Data;
using DbGate.Persist.Support.FeatureIntegration.Order;
using DbGate.Persist.Support.FeatureIntegration.Product;
using DbGate.Utility;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateFeatureIntegrationTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "testing-feature_integreation";

        public DbGateFeatureIntegrationTest()
        {
            TestClass = typeof(DbGateFeatureIntegrationTest);
            BeginInit(DbName);
        }
        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
 
        private IDbConnection SetupTables()
        {
            RegisterClassForDbPatching(typeof (ItemTransaction),DbName);
            RegisterClassForDbPatching(typeof(ItemTransactionCharge), DbName);
            RegisterClassForDbPatching(typeof(Transaction), DbName);
            RegisterClassForDbPatching(typeof(Product), DbName);
            RegisterClassForDbPatching(typeof(Service), DbName);
            EndInit(DbName);

            return Connection;
        }

        [Fact]
        public void FeatureIntegration_PersistAndRetrieve_WithComplexStructure_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var transId = 35;
                var productId = 135;
                var serviceId = 235;

                var connection = SetupTables();
                
                var product = CreateDefaultProduct(connection, productId);
                var service = CreateDefaultService(connection, serviceId);
                var transaction = CreateDefaultTransaction(connection, transId, product, service);

                var tx = CreateTransaction(connection);
                var loadedTransaction = new Transaction();
                LoadWithId(tx, loadedTransaction, transId);
                tx.Commit();
                DbMgtUtility.Close(connection);

                VerifyEquals(transaction, loadedTransaction);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private void VerifyEquals(Transaction transaction, Transaction loadedTransaction)
        {
            Assert.Equal(loadedTransaction.Name, transaction.Name);
            foreach (var orgItemTransaction in transaction.ItemTransactions)
            {
                var foundItem = false;
                foreach (var loadedItemTransaction in loadedTransaction.ItemTransactions)
                {
                    if (orgItemTransaction.IndexNo == loadedItemTransaction.IndexNo)
                    {
                        foundItem = true;
                        var originalItem = orgItemTransaction.Item;
                        var loadedItem = loadedItemTransaction.Item;

                        Assert.Equal(originalItem.GetType(), loadedItem.GetType());
                        Assert.Equal(originalItem.Name, loadedItem.Name);
                        Assert.Equal(originalItem.ItemId, loadedItem.ItemId);
                        Assert.Same(loadedItemTransaction.Transaction, loadedTransaction);

                        foreach (var orgTransactionCharge in orgItemTransaction.ItemTransactionCharges
                            )
                        {
                            var foundCharge = false;
                            foreach (
                                var loadedTransactionCharge in
                                    loadedItemTransaction.ItemTransactionCharges)
                            {
                                if (orgTransactionCharge.IndexNo == loadedTransactionCharge.IndexNo
                                    && orgTransactionCharge.ChargeIndex == loadedTransactionCharge.ChargeIndex)
                                {
                                    foundCharge = true;
                                    Assert.Equal(orgTransactionCharge.ChargeCode, loadedTransactionCharge.ChargeCode);
                                    Assert.Same(loadedTransactionCharge.Transaction, loadedTransaction);
                                    Assert.Same(loadedTransactionCharge.ItemTransaction, loadedItemTransaction);
                                }
                            }
                            Assert.True(foundCharge, "Item transaction charge not found");
                        }
                    }
                }
                Assert.True(foundItem, "Item transaction not found");
            }
            Assert.Equal(loadedTransaction.Name, transaction.Name);
        }

        private bool LoadWithId(ITransaction transaction, Transaction loadEntity, int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from order_transaction where transaction_id = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            var dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, transaction);
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
            var transaction = CreateTransaction(con);
            product.Persist(transaction);
            transaction.Commit();
            return product;
        }

        public Service CreateDefaultService(IDbConnection con, int serviceId)
        {
            var service = new Service();
            service.ItemId = serviceId;
            service.Name = "Service";
            service.HourlyRate = 65;
            var transaction = CreateTransaction(con);
            service.Persist(transaction);
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

            var tx = CreateTransaction(con);
            transaction.Persist(tx);
            tx.Commit();
            return transaction;
        }
    }
}