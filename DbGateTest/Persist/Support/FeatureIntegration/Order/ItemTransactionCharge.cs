namespace DbGate.Persist.Support.FeatureIntegration.Order
{
    [TableInfo("order_item_transaction_charge")]
    public class ItemTransactionCharge : DefaultEntity
    {
        public ItemTransactionCharge()
        {
        }

        public ItemTransactionCharge(ItemTransaction itemTransaction)
        {
            ItemTransaction = itemTransaction;
            Transaction = itemTransaction.Transaction;
        }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int TransactionId { get; set; }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ChargeIndex { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string ChargeCode { get; set; }

        [ForeignKeyInfo("item_tx_charge2tx_rev", 
            typeof (Transaction),
            new[] {"transactionId"},
            new[] {"transactionId"},
            ReverseRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public Transaction Transaction { get; set; }

        [ForeignKeyInfo("item_tx_charge2tx_item_rev",
            typeof (ItemTransaction),
            new[] {"transactionId", "indexNo"},
            new[] {"transactionId", "indexNo"},
            ReverseRelation = true,
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade)]
        public ItemTransaction ItemTransaction { get; set; }
    }
}