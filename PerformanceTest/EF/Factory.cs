using System;
using System.Collections.Generic;
using System.Linq;
using PerformanceTest.EF.Entities.Order;
using PerformanceTest.EF.Entities.Product;

namespace PerformanceTest.EF
{
    public class Factory
    {
        private readonly Random random = new Random();

        public IList<object> Generate(int seed,int txCount,int productsOrServicesPerTx)
        {
            var list = new List<object>();
            var productIds = new List<int>();
            var serviceIds = new List<int>();

            var productOrServiceCount = txCount * productsOrServicesPerTx;
            for (int i = 0; i < productOrServiceCount; i++)
            {
                var product = new Product();
                product.ItemId = seed + 2 * i;
                product.Name = $"Product - {i}";
                product.UnitPrice = 54 + i;
                product.BulkUnitPrice = 104 + i;
                productIds.Add(product.ItemId);

                list.Add(product);
            }

            for (int i = 0; i < productOrServiceCount; i++)
            {
                var service = new Service();
                service.ItemId = seed + 2 * i + 1;
                service.Name = $"Service - {i}";
                service.HourlyRate = 10 + i;
                serviceIds.Add(service.ItemId);

                list.Add(service);
            }

            for (int i = 0; i < txCount; i++)
            {
                var transaction = new Transaction();
                transaction.ItemTransactions = new List<ItemTransaction>();
                transaction.TransactionId = seed + i;
                transaction.Name = $"TRS-000{i}";

                int productsCount = random.Next(1, productsOrServicesPerTx);
                for (int j = 0; j < productsCount; j++)
                {
                    var productId = productIds[random.Next(0, productIds.Count)];
                    var product = list.OfType<Product>().Single(p => p.ItemId == productId);

                    var productTransaction = new ItemTransaction();
                    productTransaction.Transaction = transaction;
                    productTransaction.ItemTransactionCharges = new List<ItemTransactionCharge>();
                    productTransaction.IndexNo = j;
                    productTransaction.Item = product;
                    transaction.ItemTransactions.Add(productTransaction);

                    var productTransactionCharge = new ItemTransactionCharge();
                    productTransactionCharge.TransactionId = transaction.TransactionId;
                    productTransactionCharge.IndexNo = productTransaction.IndexNo;
                    productTransactionCharge.ChargeIndex = 0;
                    productTransactionCharge.ItemTransaction = productTransaction;
                    productTransactionCharge.ChargeCode = $"Product-Sell-Code {i} {j}";
                    productTransaction.ItemTransactionCharges.Add(productTransactionCharge);
                }

                int servicesCount = random.Next(1, productsOrServicesPerTx);
                for (int j = 0; j < servicesCount; j++)
                {
                    var serviceId = serviceIds[random.Next(0, serviceIds.Count)];
                    var service = list.OfType<Service>().Single(p => p.ItemId == serviceId);

                    var serviceTransaction = new ItemTransaction();
                    serviceTransaction.Transaction = transaction;
                    serviceTransaction.ItemTransactionCharges = new List<ItemTransactionCharge>();
                    serviceTransaction.IndexNo = productsCount + j + 1;
                    serviceTransaction.Item = service;
                    transaction.ItemTransactions.Add(serviceTransaction);

                    var serviceTransactionCharge = new ItemTransactionCharge();
                    serviceTransactionCharge.TransactionId = transaction.TransactionId;
                    serviceTransactionCharge.IndexNo = serviceTransaction.IndexNo;
                    serviceTransactionCharge.ChargeIndex = 0;
                    serviceTransactionCharge.ItemTransaction = serviceTransaction;
                    serviceTransactionCharge.ChargeCode = $"Service-Sell-Code {i} {j}";
                    serviceTransaction.ItemTransactionCharges.Add(serviceTransactionCharge);
                }

                list.Add(transaction);
            }

            return list;
        }

        public void Update(object entity)
        {
            if (entity is Product product)
            {
                product.Name = "Upd " + product.Name;
                product.UnitPrice += 5;
                product.BulkUnitPrice += 2;
            }
            if (entity is Service service)
            {
                service.Name = "Upd " + service.Name;
                service.HourlyRate += 2;
            }
            if (entity is Transaction transaction)
            {
                transaction.Name = "Upd " + transaction.Name;
                var itemTransactions = transaction.ItemTransactions.ToArray();

                for (int i = 0; i < itemTransactions.Length; i++)
                {
                    var itemTx = itemTransactions.ToArray()[i];
                    itemTx.Item = itemTransactions[itemTransactions.Length -1 - i].Item;

                    foreach (var chg in itemTx.ItemTransactionCharges)
                    {
                        chg.ChargeCode = "Upd " + chg.ChargeCode;
                    }
                }
            }
        }
    }
}

