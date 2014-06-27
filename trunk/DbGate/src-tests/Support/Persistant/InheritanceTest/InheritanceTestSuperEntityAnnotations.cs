namespace DbGate.Support.Persistant.InheritanceTest
{
    [TableInfo("inheritance_test_super")]
    public class InheritanceTestSuperEntityAnnotations : DefaultEntity, IInheritanceTestSuperEntity
    {
        #region IInheritanceTestSuperEntity Members

        [ColumnInfo((ColumnType.Integer), Key = true, SubClassCommonColumn = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }

        #endregion
    }
}