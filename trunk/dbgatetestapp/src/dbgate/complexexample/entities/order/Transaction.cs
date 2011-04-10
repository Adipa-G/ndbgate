using System.Collections.Generic;
using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.complexexample.entities.order
{
    [DbTableInfo("order_transaction")]
    public class Transaction : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer,Key = true)]
        public int TransactionId { get; set; }
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("tx2item_tx", typeof(dbgatetestapp.dbgate.complexexample.entities.order.ItemTransaction), new string[] { "transactionId" },new string[] { "transactionId"}
            , UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<dbgatetestapp.dbgate.complexexample.entities.order.ItemTransaction> ItemTransactions { get; set; }

        public Transaction()
        {
            ItemTransactions = new List<dbgatetestapp.dbgate.complexexample.entities.order.ItemTransaction>();
        }
    }
}
