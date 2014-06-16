namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    [TableInfo("inheritance_test_subb")]
    public class InheritanceTestSubEntityBAnnotations : InheritanceTestSuperEntityAnnotations , IInheritanceTestSubEntityB
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string NameB { get; set; }
    }
}