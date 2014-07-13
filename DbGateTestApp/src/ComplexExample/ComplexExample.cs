using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.ComplexExample.Entities.Order;
using DbGateTestApp.ComplexExample.Entities.Product;

namespace DbGateTestApp.ComplexExample
{
    public class ComplexExample
    {
        public const int ProductId = 43;
        public const int ServiceId = 63;
        public const int TransactionId = 1243;

        public Product CreateDefaultProduct(IDbConnection con)
        {
            var product = new Product();
            product.ItemId = ProductId;
            product.Name = "Product";
            product.UnitPrice = 54;
            IDbTransaction transaction = con.BeginTransaction();
            product.Persist(con);
            transaction.Commit();
            return product;
        }

        public Service CreateDefaultService(IDbConnection con)
        {
            var service = new Service();
            service.ItemId = ServiceId;
            service.Name = "Service";
            service.HourlyRate = 65;
            IDbTransaction transaction = con.BeginTransaction();
            service.Persist(con);
            transaction.Commit();
            return service;
        }

        public Transaction CreateDefaultTransaction(IDbConnection con, Product product, Service service)
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

            IDbTransaction dbTransaction = con.BeginTransaction();
            transaction.Persist(con);
            dbTransaction.Commit();
            return transaction;
        }

        public void Patch(IDbConnection con)
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof (Product));
            entityTypes.Add(typeof (Service));
            entityTypes.Add(typeof (Transaction));
            entityTypes.Add(typeof (ItemTransaction));
            entityTypes.Add(typeof (ItemTransactionCharge));
            IDbTransaction transaction = con.BeginTransaction();
            DbGate._transactionFactory.DbGate.PatchDataBase(con, entityTypes, false);
            transaction.Commit();
        }

        public void Persist(IDbConnection con, IEntity entity)
        {
            IDbTransaction transaction = con.BeginTransaction();
            entity.Persist(con);
            transaction.Commit();
        }

        public Transaction Retrieve(IDbConnection con)
        {
            IDbCommand cmd = con.CreateCommand();
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
                entity.Retrieve(reader, con);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            var example = new ComplexExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            Product product = example.CreateDefaultProduct(con);
            example.Persist(con, product);

            Service service = example.CreateDefaultService(con);
            example.Persist(con, service);

            Transaction transaction = example.CreateDefaultTransaction(con, product, service);
            example.Persist(con, transaction);

            transaction = example.Retrieve(con);
            Console.WriteLine("Transaction Name = " + transaction.Name);
            foreach (ItemTransaction itemTransaction in transaction.ItemTransactions)
            {
                Console.WriteLine("Item Name = " + itemTransaction.Item.Name);
            }
            ExampleBase.CloseDb();
        }
    }
}