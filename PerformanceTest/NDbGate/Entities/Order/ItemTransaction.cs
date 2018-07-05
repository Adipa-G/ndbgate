using System;
using System.Collections.Generic;
using DbGate;
using PerformanceTest.NDbGate.Entities.Product;

namespace PerformanceTest.NDbGate.Entities.Order
{
    [TableInfo("dbgate_order_item_transaction")]
    public class ItemTransaction : DefaultEntity
    {
        public ItemTransaction(Transaction transaction)
        {
            Transaction = transaction;
            ItemTransactionCharges = new List<ItemTransactionCharge>();
        }

        public ItemTransaction()
        {
            ItemTransactionCharges = new List<ItemTransactionCharge>();
        }

        [ColumnInfo(ColumnType.Guid, Key = true)]
        public Guid TransactionId { get; set; }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo(ColumnType.Integer)]
        public int ItemId { get; set; }

        [ForeignKeyInfo("item_tx2item",
            typeof (Item), 
            new[] {"itemId"},
            new[] {"itemId"}, 
            NonIdentifyingRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public Item Item { get; set; }

        [ForeignKeyInfo("tx2item_tx",
            typeof (Transaction),
            new[] {"transactionId"},
            new[] {"transactionId"},
            ReverseRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public Transaction Transaction { get; set; }

        [ForeignKeyInfo("item_tx2tx_chg",
            typeof (ItemTransactionCharge),
            new[] {"transactionId", "indexNo"},
            new[] {"transactionId", "indexNo"},
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ItemTransactionCharge> ItemTransactionCharges { get; set; }
    }
}