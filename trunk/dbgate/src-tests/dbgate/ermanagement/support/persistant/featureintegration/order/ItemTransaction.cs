using System.Collections.Generic;
using dbgate.ermanagement.support.persistant.featureintegration.product;

namespace dbgate.ermanagement.support.persistant.featureintegration.order
{
    [DbTableInfo("order_item_transaction")]
    public class ItemTransaction  : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer,Key = true)]
        public int TransactionId { get; set; }
        [DbColumnInfo(DbColumnType.Integer,Key = true)]
        public int IndexNo { get; set; }
        [DbColumnInfo(DbColumnType.Integer)]
        public int ItemId { get; set; }

        [ForeignKeyInfo("item_tx2product", typeof (Product), new string[] {"itemId"}, new string[] {"itemId"}
            , NonIdentifyingRelation = true, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        [ForeignKeyInfo("item_tx2service", typeof (Service), new string[] {"itemId"}, new string[] {"itemId"}
            , NonIdentifyingRelation = true, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public Item Item { get; set; }

        [ForeignKeyInfo("item_tx2tx_rev", typeof (Transaction), new string[] {"transactionId"},new string[] {"transactionId"}
            , ReverseRelation = true, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public Transaction Transaction { get; set; }

        [ForeignKeyInfo("item_tx2tx_chg", typeof(ItemTransactionCharge), new string[] { "transactionId", "indexNo" },
            new string[] { "transactionId", "indexNo" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ItemTransactionCharge> ItemTransactionCharges { get; set; }

        public ItemTransaction(Transaction transaction)
        {
            this.Transaction = transaction;
            ItemTransactionCharges = new List<ItemTransactionCharge>();
        }

        public ItemTransaction()
        {
            ItemTransactionCharges = new List<ItemTransactionCharge>();
        }
    }
}
