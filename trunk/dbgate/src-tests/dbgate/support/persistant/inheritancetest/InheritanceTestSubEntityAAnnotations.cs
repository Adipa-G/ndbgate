using dbgate.ermanagement;

namespace dbgate.support.persistant.inheritancetest
{
    [TableInfo("inheritance_test_suba")]
    public class InheritanceTestSubEntityAAnnotations : InheritanceTestSuperEntityAnnotations ,IInheritanceTestSubEntityA
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string NameA { get; set; }
    }
}