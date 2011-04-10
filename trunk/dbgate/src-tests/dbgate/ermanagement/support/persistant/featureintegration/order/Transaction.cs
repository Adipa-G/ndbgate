using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.featureintegration.order
{
    [DbTableInfo("order_transaction")]
    public class Transaction : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer,Key = true)]
        public int TransactionId { get; set; }
        [DbColumnInfo(DbColumnType.Varchar)]
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
