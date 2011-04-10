namespace dbgate.ermanagement.support.persistant.constraint
{
    [DbTableInfo("constraint_test_one2one")]
    public class ConstraintTestOne2OneEntity : DefaultServerDbClass
    {
        public ConstraintTestOne2OneEntity()
        {
        }

        [DbColumnInfo((DbColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }
    }
}
