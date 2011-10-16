namespace dbgate.ermanagement.support.patch.patchtabledifferences
{
    [DbTableInfo("table_change_test_entity")]
    public class ThreeColumnTypeDifferentEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [DbColumnInfo((DbColumnType.Varchar),Size = 255)]
        public string Name { get; set; }
    }
}
