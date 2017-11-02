using System;
using System.Collections.Generic;
using System.Linq;
using DbGate;
using PerformanceTest.NDbGate.Entities.Order;
using PerformanceTest.NDbGate.Entities.Product;

namespace PerformanceTest.NDbGate
{
    public class Factory
    {
        private Random random = new Random();

        public IList<IEntity> Generate(int seed,int txCount,int productsOrServicesPerTx)
        {
            var list = new List<IEntity>();
            var productIds = new List<int>();
            var serviceIds = new List<int>();

            var productOrCount = txCount * productsOrServicesPerTx;
            for (int i = 0; i < productOrCount; i++)
            {
                var product = new Product();
                product.ItemId = seed + 2 * i;
                product.Name = $"Product - {i}";
                product.UnitPrice = 54 + i;
                product.BulkUnitPrice = 104 + i;
                productIds.Add(product.ItemId);

                list.Add(product);

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
                transaction.TransactionId = seed + i;
                transaction.Name = $"TRS-000{i}";

                int productsCount = random.Next(1, productsOrServicesPerTx);
                for (int j = 0; j < productsCount; j++)
                {
                    var productId = productIds[random.Next(0, productIds.Count)];
                    var product = list.OfType<Product>().Single(p => p.ItemId == productId);

                    var productTransaction = new ItemTransaction(transaction);
                    productTransaction.IndexNo = j;
                    productTransaction.Item = product;
                    transaction.ItemTransactions.Add(productTransaction);

                    var productTransactionCharge = new ItemTransactionCharge(productTransaction);
                    productTransactionCharge.ChargeCode = $"Product-Sell-Code {i} {j}";
                    productTransaction.ItemTransactionCharges.Add(productTransactionCharge);
                }

                int servicesCount = random.Next(1, productsOrServicesPerTx);
                for (int j = 0; j < servicesCount; j++)
                {
                    var serviceId = serviceIds[random.Next(0, serviceIds.Count)];
                    var service = list.OfType<Service>().Single(p => p.ItemId == serviceId);

                    var serviceTransaction = new ItemTransaction(transaction);
                    serviceTransaction.IndexNo = productsCount + j + 1;
                    serviceTransaction.Item = service;
                    transaction.ItemTransactions.Add(serviceTransaction);

                    var serviceTransactionCharge = new ItemTransactionCharge(serviceTransaction);
                    serviceTransactionCharge.ChargeCode = $"Service-Sell-Code {i} {j}";
                    serviceTransaction.ItemTransactionCharges.Add(serviceTransactionCharge);
                }

                list.Add(transaction);
            }

            return list;
        }
    }
}
