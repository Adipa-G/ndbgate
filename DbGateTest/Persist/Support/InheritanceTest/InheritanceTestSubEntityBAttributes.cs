namespace DbGate.Persist.Support.InheritanceTest
{
    [TableInfo("inheritance_test_subb")]
    public class InheritanceTestSubEntityBAttributes : InheritanceTestSuperEntityAttributes,
                                                        IInheritanceTestSubEntityB
    {
        #region IInheritanceTestSubEntityB Members

        [ColumnInfo(ColumnType.Varchar)]
        public string NameB { get; set; }

        #endregion
    }
}