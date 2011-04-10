namespace dbgate.ermanagement.support.persistant.treetest
{
    [DbTableInfo("tree_test_one2one")]
    public class TreeTestOne2OneEntityAnnotations : DefaultServerDbClass , ITreeTestOne2OneEntity
    {
        [DbColumnInfo((DbColumnType.Integer),Key = true)]
        public int IdCol { get; set; }
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        public TreeTestOne2OneEntityAnnotations()
        {
        }
    }
}