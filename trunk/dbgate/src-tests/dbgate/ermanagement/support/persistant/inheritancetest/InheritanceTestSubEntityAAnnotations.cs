namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    [DbTableInfo("inheritance_test_suba")]
    public class InheritanceTestSubEntityAAnnotations : InheritanceTestSuperEntityAnnotations ,IInheritanceTestSubEntityA
    {
        [DbColumnInfo(DbColumnType.Varchar)]
        public string NameA { get; set; }
    }
}