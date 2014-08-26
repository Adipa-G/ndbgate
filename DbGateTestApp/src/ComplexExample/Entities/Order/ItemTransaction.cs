using System.Collections.Generic;
using DbGate;
using DbGateTestApp.ComplexExample.Entities.Product;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.ComplexExample.Entities.Order
{
    [WikiCodeBlock("complex_example_order_item_transaction")]
    [TableInfo("order_item_transaction")]
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

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int TransactionId { get; set; }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo(ColumnType.Integer)]
        public int ItemId { get; set; }

        [ForeignKeyInfo("item_tx2product", typeof (Product.Product), new[] {"itemId"}, new[] {"itemId"}
            , NonIdentifyingRelation = true, UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        [ForeignKeyInfo("item_tx2service", typeof (Service), new[] {"itemId"}, new[] {"itemId"}
            , NonIdentifyingRelation = true, UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public Item Item { get; set; }

        [ForeignKeyInfo("item_tx2tx_rev", typeof (Transaction), new[] {"transactionId"}, new[] {"transactionId"}
            , ReverseRelation = true, UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public Transaction Transaction { get; set; }

        [ForeignKeyInfo("item_tx2tx_chg", typeof (ItemTransactionCharge), new[] {"transactionId", "indexNo"},
            new[] {"transactionId", "indexNo"}, UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ItemTransactionCharge> ItemTransactionCharges { get; set; }
    }
}