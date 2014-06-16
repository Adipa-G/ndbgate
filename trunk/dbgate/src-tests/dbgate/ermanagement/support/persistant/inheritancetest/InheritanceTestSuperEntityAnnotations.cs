namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    [TableInfo("inheritance_test_super")]
    public class InheritanceTestSuperEntityAnnotations : DefaultEntity , IInheritanceTestSuperEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true, SubClassCommonColumn = true)]
        public int IdCol { get; set; }
        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }

        public InheritanceTestSuperEntityAnnotations()
        {
        }
    }
}