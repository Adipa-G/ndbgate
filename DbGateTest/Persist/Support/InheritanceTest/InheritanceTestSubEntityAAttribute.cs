namespace DbGate.Persist.Support.InheritanceTest
{
    [TableInfo("inheritance_test_suba")]
    public class InheritanceTestSubEntityAAttribute : InheritanceTestSuperEntityAttributes,
                                                        IInheritanceTestSubEntityA
    {
        #region IInheritanceTestSubEntityA Members

        [ColumnInfo(ColumnType.Varchar)]
        public string NameA { get; set; }

        #endregion
    }
}