using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.ComplexExample.Entities.Order;
using DbGateTestApp.ComplexExample.Entities.Product;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.ComplexExample
{
    [WikiCodeBlock("complex_example")]
    public class ComplexExample
    {
        public const int ProductId = 43;
        public const int ServiceId = 63;
        public const int TransactionId = 1243;

        public Product CreateDefaultProduct(ITransaction tx)
        {
            var product = new Product();
            product.ItemId = ProductId;
            product.Name = "Product";
            product.UnitPrice = 54;
            product.Persist(tx);
            return product;
        }

        public Service CreateDefaultService(ITransaction tx)
        {
            var service = new Service();
            service.ItemId = ServiceId;
            service.Name = "Service";
            service.HourlyRate = 65;
            service.Persist(tx);
            return service;
        }

        public Transaction CreateDefaultTransaction(ITransaction tx, Product product, Service service)
        {
            var transaction = new Transaction();
            transaction.TransactionId = TransactionId;
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

            transaction.Persist(tx);
            return transaction;
        }

        public void Patch(ITransaction tx)
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof (Product));
            entityTypes.Add(typeof (Service));
            entityTypes.Add(typeof (Transaction));
            entityTypes.Add(typeof (ItemTransaction));
            entityTypes.Add(typeof (ItemTransactionCharge));

            tx.DbGate.PatchDataBase(tx, entityTypes, false);
        }

        public void Persist(ITransaction tx, IEntity entity)
        {
            entity.Persist(tx);
        }

        public Transaction Retrieve(ITransaction tx)
        {
            IDbCommand cmd = tx.CreateCommand();
            cmd.CommandText = "select * from order_transaction where transaction_id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = TransactionId;

            Transaction entity = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                entity = new Transaction();
                entity.Retrieve(reader, tx);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            var example = new ComplexExample();
            ITransaction tx = ExampleBase.SetupDb();
            example.Patch(tx);

            Product product = example.CreateDefaultProduct(tx);
            example.Persist(tx, product);

            Service service = example.CreateDefaultService(tx);
            example.Persist(tx, service);

            Transaction transaction = example.CreateDefaultTransaction(tx, product, service);
            example.Persist(tx, transaction);

            transaction = example.Retrieve(tx);
            Console.WriteLine("Transaction Name = " + transaction.Name);
            foreach (ItemTransaction itemTransaction in transaction.ItemTransactions)
            {
                Console.WriteLine("Item Name = " + itemTransaction.Item.Name);
            }
            ExampleBase.CloseDb();
        }
    }
}