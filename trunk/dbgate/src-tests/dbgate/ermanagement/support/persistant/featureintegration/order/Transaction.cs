using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.featureintegration.order
{
    [TableInfo("order_transaction")]
    public class Transaction : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer,Key = true)]
        public int TransactionId { get; set; }
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("tx2item_tx", typeof(ItemTransaction), new string[] { "transactionId" },new string[] { "transactionId"}
            , UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<ItemTransaction> ItemTransactions { get; set; }

        public Transaction()
        {
            ItemTransactions = new List<ItemTransaction>();
        }
    }
}
