namespace DbGate.Persist.Support.NonIdentifyingRelationWithoutColumn
{
    [TableInfo("relation_test_currency")]
    public class Currency : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int CurrencyId { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Code { get; set; }
    }
}
