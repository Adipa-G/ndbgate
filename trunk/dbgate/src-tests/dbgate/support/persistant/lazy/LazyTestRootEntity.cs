using System.Collections.Generic;
using dbgate.ermanagement;

namespace dbgate.support.persistant.lazy
{
    [TableInfo("lazy_test_root")]
    public class LazyTestRootEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public virtual string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",typeof(LazyTestOne2ManyEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade,Lazy = true)]
        public virtual ICollection<LazyTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent",typeof(LazyTestOne2OneEntity),new string[]{"idCol"}
            ,new string[]{"idCol"},UpdateRule = ReferentialRuleType.Restrict,DeleteRule = ReferentialRuleType.Cascade,Lazy = true)]
        public virtual LazyTestOne2OneEntity One2OneEntity { get; set; }

        public LazyTestRootEntity()
        {
            One2ManyEntities = new List<LazyTestOne2ManyEntity>();
        }
    }
}
