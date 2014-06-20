using dbgate.ermanagement;

namespace dbgate.support.persistant.featureintegration.order
{
    [TableInfo("order_item_transaction_charge")]
    public class ItemTransactionCharge  : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int TransactionId { get; set; }
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ChargeIndex { get; set; }
        [ColumnInfo(ColumnType.Varchar)]
        public string ChargeCode { get; set; }

        [ForeignKeyInfo("item_tx_charge2tx_rev", typeof (Transaction), new string[] {"transactionId"},new string[] {"transactionId"}
            , ReverseRelation = true, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public Transaction Transaction { get; set; }

        [ForeignKeyInfo("item_tx_charge2tx_item_rev", typeof (ItemTransaction), new string[] {"transactionId","indexNo"},new string[] {"transactionId","indexNo"}
            , ReverseRelation = true, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ItemTransaction ItemTransaction { get; set; }

        public ItemTransactionCharge()
        {
        }

        public ItemTransactionCharge(ItemTransaction itemTransaction)
        {
            this.ItemTransaction = itemTransaction;
            this.Transaction = itemTransaction.Transaction;
        }
    }
}
