namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    [DbTableInfo("inheritance_test_subb")]
    public class InheritanceTestSubEntityBAnnotations : InheritanceTestSuperEntityAnnotations , IInheritanceTestSubEntityB
    {
        [DbColumnInfo(DbColumnType.Varchar)]
        public string NameB { get; set; }
    }
}