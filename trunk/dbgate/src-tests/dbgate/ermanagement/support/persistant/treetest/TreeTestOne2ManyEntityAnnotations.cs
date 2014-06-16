namespace dbgate.ermanagement.support.persistant.treetest
{
    [TableInfo("tree_test_one2many")]
    public class TreeTestOne2ManyEntityAnnotations : DefaultEntity , ITreeTestOne2ManyEntity
    {
        [ColumnInfo(( ColumnType.Integer),Key = true)]
        public int IdCol { get; set; }
        [ColumnInfo(( ColumnType.Integer),Key = true)]
        public int IndexNo { get; set; }
        [ColumnInfo( ColumnType.Varchar)]
        public string Name { get; set; } 

        public TreeTestOne2ManyEntityAnnotations()
        {
        }
    }
}
