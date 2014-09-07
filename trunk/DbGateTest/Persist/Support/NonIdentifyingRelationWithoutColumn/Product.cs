namespace DbGate.Persist.Support.NonIdentifyingRelationWithoutColumn
{
    [TableInfo("relation_test_product")]
    public class Product : DefaultEntity
    {
        public Product()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int ProductId { get; set; }

        [ColumnInfo(ColumnType.Double)]
        public virtual double Price { get; set; }

        [ForeignKeyInfo("product2currency", typeof(Currency), new[] { "productCurrencyId" }
            , new[] {"CurrencyId"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade,
            NonIdentifyingRelation = true,Nullable = true)]
        public virtual Currency Currency { get; set; }
    }
}