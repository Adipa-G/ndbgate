using System;
using System.Collections.Generic;
using DbGate;

namespace PerformanceTest.NDbGate.Entities.Order
{
    [TableInfo("dbgate_order_transaction")]
    public class Transaction : DefaultEntity
    {
        public Transaction()
        {
            ItemTransactions = new List<ItemTransaction>();
        }

        [ColumnInfo(ColumnType.Guid, Key = true)]
        public Guid TransactionId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("tx2item_tx",
            typeof (ItemTransaction),
            new[] {"transactionId"},
            new[] {"transactionId"},
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ItemTransaction> ItemTransactions { get; set; }
    }
}