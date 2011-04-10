namespace dbgate.ermanagement.support.persistant.constraint
{
    [DbTableInfo("constraint_test_one2many")]
    public class ConstraintTestOne2ManyEntity : DefaultServerDbClass
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IndexNo { get; set; }

        [DbColumnInfo((DbColumnType.Varchar))]
        public string Name { get; set; }
    }
}

