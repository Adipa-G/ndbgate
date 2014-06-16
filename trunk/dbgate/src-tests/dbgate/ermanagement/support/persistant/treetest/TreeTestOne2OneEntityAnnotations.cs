namespace dbgate.ermanagement.support.persistant.treetest
{
    [TableInfo("tree_test_one2one")]
    public class TreeTestOne2OneEntityAnnotations : DefaultEntity , ITreeTestOne2OneEntity
    {
        [ColumnInfo((ColumnType.Integer),Key = true)]
        public int IdCol { get; set; }
        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        public TreeTestOne2OneEntityAnnotations()
        {
        }
    }
}