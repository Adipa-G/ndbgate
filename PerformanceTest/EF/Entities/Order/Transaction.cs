using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerformanceTest.EF.Entities.Order
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TransactionId { get; set; }

        public string Name { get; set; }

        public ICollection<ItemTransaction> ItemTransactions { get; set; }
    }
}
