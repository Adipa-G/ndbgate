namespace dbgate.ermanagement.support.persistant.inheritancetest
{
    [DbTableInfo("inheritance_test_super")]
    public class InheritanceTestSuperEntityAnnotations : DefaultServerDbClass , IInheritanceTestSuperEntity
    {
        [DbColumnInfo((DbColumnType.Integer), Key = true, SubClassCommonColumn = true)]
        public int IdCol { get; set; }
        [DbColumnInfo((DbColumnType.Varchar))]
        public string Name { get; set; }

        public InheritanceTestSuperEntityAnnotations()
        {
        }
    }
}