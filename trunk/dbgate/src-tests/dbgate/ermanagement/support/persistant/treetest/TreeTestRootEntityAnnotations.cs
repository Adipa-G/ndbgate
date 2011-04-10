using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.treetest
{
    [DbTableInfo("tree_test_root")]
    public class TreeTestRootEntityAnnotations : DefaultServerDbClass , ITreeTestRootEntity
    {
        [DbColumnInfo((DbColumnType.Integer),Key = true)]
        public int IdCol { get; set; }
        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",typeof(TreeTestOne2ManyEntityAnnotations),new []{"idcol"},new []{"idcol"}
            ,DeleteRule = ReferentialRuleType.Cascade,UpdateRule = ReferentialRuleType.Restrict)]    
        public List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent",typeof(TreeTestOne2OneEntityAnnotations),new []{"idcol"},new []{"idcol"}
            ,DeleteRule = ReferentialRuleType.Cascade,UpdateRule = ReferentialRuleType.Restrict)]    
        public ITreeTestOne2OneEntity One2OneEntity { get; set; }

        public TreeTestRootEntityAnnotations()
        {
        }
    }
}
