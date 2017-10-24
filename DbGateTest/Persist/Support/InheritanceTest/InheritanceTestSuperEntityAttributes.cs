namespace DbGate.Persist.Support.InheritanceTest
{
    [TableInfo("inheritance_test_super")]
    public class InheritanceTestSuperEntityAttributes : DefaultEntity, IInheritanceTestSuperEntity
    {
        #region IInheritanceTestSuperEntity Members

        [ColumnInfo((ColumnType.Integer), Key = true, SubClassCommonColumn = true)]
        public int IdCol { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Name { get; set; }

        #endregion
    }
}