namespace dbgate.ermanagement.support.persistant.constraint
{
    [TableInfo("constraint_test_one2one")]
    public class ConstraintTestOne2OneEntity : DefaultEntity
    {
        public ConstraintTestOne2OneEntity()
        {
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }
    
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}
