namespace DbGate.Support.Persistant.Constraint
{
    [TableInfo("constraint_test_one2one")]
    public class ConstraintTestOne2OneEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}