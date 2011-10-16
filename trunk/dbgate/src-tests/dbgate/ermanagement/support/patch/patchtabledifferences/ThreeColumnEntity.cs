namespace dbgate.ermanagement.support.patch.patchtabledifferences
{
    [DbTableInfo("table_change_test_entity")]
    public class ThreeColumnEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [DbColumnInfo((DbColumnType.Varchar),Size = 10)]
        public string Name { get; set; }
    }
}
