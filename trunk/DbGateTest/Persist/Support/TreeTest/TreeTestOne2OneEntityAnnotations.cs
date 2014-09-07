namespace DbGate.Persist.Support.TreeTest
{
    [TableInfo("tree_test_one2one")]
    public class TreeTestOne2OneEntityAnnotations : DefaultEntity, ITreeTestOne2OneEntity
    {
        #region ITreeTestOne2OneEntity Members

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        #endregion
    }
}