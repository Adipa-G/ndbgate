using System;
using System.Collections.Generic;
using System.Data;
using dbgate;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgatetestapp.dbgate.complexexample.entities.order;
using dbgatetestapp.dbgate.complexexample.entities.product;

namespace dbgatetestapp.dbgate.complexexample
{
    public class ComplexExample
    {
        public const int ProductId = 43;
        public const int ServiceId = 63;
        public const int TransactionId = 1243;

        public Product CreateDefaultProduct(IDbConnection con)
        {
            Product product = new Product();
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
            Service service = new Service();
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
            Transaction transaction = new Transaction();
            transaction.TransactionId = TransactionId;
            transaction.Name = "TRS-0001";

            ItemTransaction productTransaction = new ItemTransaction(transaction);
            productTransaction.IndexNo = 0;
            productTransaction.Item = product;
            transaction.ItemTransactions.Add(productTransaction);

            ItemTransactionCharge productTransactionCharge = new ItemTransactionCharge(productTransaction);
            productTransactionCharge.ChargeCode = "Product-Sell-Code";
            productTransaction.ItemTransactionCharges.Add(productTransactionCharge);

            ItemTransaction serviceTransaction = new ItemTransaction(transaction);
            serviceTransaction.IndexNo = 1;
            serviceTransaction.Item = service;
            transaction.ItemTransactions.Add(serviceTransaction);

            ItemTransactionCharge serviceTransactionCharge = new ItemTransactionCharge(serviceTransaction);
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
            entityTypes.Add(typeof(Product));
            entityTypes.Add(typeof(Service));
            entityTypes.Add(typeof(Transaction));
            entityTypes.Add(typeof(ItemTransaction));
            entityTypes.Add(typeof(ItemTransactionCharge));
            IDbTransaction transaction = con.BeginTransaction();
            ErLayer.GetSharedInstance().PatchDataBase(con,entityTypes,false);
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
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            ComplexExample example = new ComplexExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            Product product = example.CreateDefaultProduct(con);
            example.Persist(con,product);

            Service service = example.CreateDefaultService(con);
            example.Persist(con, service);

            Transaction transaction = example.CreateDefaultTransaction(con,product,service);
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


