namespace dbgate.ermanagement.support.persistant.treetest
{
    [DbTableInfo("tree_test_one2many")]
    public class TreeTestOne2ManyEntityAnnotations : DefaultServerDbClass , ITreeTestOne2ManyEntity
    {
        [DbColumnInfo(( DbColumnType.Integer),Key = true)]
        public int IdCol { get; set; }
        [DbColumnInfo(( DbColumnType.Integer),Key = true)]
        public int IndexNo { get; set; }
        [DbColumnInfo( DbColumnType.Varchar)]
        public string Name { get; set; } 

        public TreeTestOne2ManyEntityAnnotations()
        {
        }
    }
}
