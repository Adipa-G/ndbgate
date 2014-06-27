using System.Collections.Generic;

namespace DbGate.Support.Persistant.FeatureIntegration.Order
{
    [TableInfo("order_transaction")]
    public class Transaction : DefaultEntity
    {
        public Transaction()
        {
            ItemTransactions = new List<ItemTransaction>();
        }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int TransactionId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("tx2item_tx", typeof (ItemTransaction), new[] {"transactionId"}, new[] {"transactionId"}
            , UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ItemTransaction> ItemTransactions { get; set; }
    }
}