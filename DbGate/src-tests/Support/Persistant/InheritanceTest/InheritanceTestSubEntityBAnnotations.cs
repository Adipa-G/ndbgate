namespace DbGate.Support.Persistant.InheritanceTest
{
    [TableInfo("inheritance_test_subb")]
    public class InheritanceTestSubEntityBAnnotations : InheritanceTestSuperEntityAnnotations,
                                                        IInheritanceTestSubEntityB
    {
        #region IInheritanceTestSubEntityB Members

        [ColumnInfo(ColumnType.Varchar)]
        public string NameB { get; set; }

        #endregion
    }
}