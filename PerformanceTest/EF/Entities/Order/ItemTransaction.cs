using System.Collections.Generic;
using PerformanceTest.EF.Entities.Product;

namespace PerformanceTest.EF.Entities.Order
{
    public class ItemTransaction
    {
        public int TransactionId { get; set; }

        public int IndexNo { get; set; }

        public Item Item { get; set; }

        public Transaction Transaction { get; set; }

        public ICollection<ItemTransactionCharge> ItemTransactionCharges { get; set; }
    }
}
