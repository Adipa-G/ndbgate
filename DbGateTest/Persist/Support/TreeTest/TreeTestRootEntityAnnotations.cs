using System.Collections.Generic;

namespace DbGate.Persist.Support.TreeTest
{
    [TableInfo("tree_test_root")]
    public class TreeTestRootEntityAnnotations : DefaultEntity, ITreeTestRootEntity
    {
        #region ITreeTestRootEntity Members

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof (TreeTestOne2ManyEntityAnnotations), new[] {"idcol"},
            new[] {"idcol"}
            , DeleteRule = ReferentialRuleType.Cascade, UpdateRule = ReferentialRuleType.Restrict)]
        public List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof (TreeTestOne2OneEntityAnnotations), new[] {"idcol"},
            new[] {"idcol"}
            , DeleteRule = ReferentialRuleType.Cascade, UpdateRule = ReferentialRuleType.Restrict)]
        public ITreeTestOne2OneEntity One2OneEntity { get; set; }

        #endregion
    }
}