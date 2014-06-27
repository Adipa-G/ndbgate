using System.Collections.Generic;

namespace DbGate.Support.Persistant.Lazy
{
    [TableInfo("lazy_test_root")]
    public class LazyTestRootEntity : DefaultEntity
    {
        public LazyTestRootEntity()
        {
            One2ManyEntities = new List<LazyTestOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public virtual string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent", typeof (LazyTestOne2ManyEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade,
            Lazy = true)]
        public virtual ICollection<LazyTestOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent", typeof (LazyTestOne2OneEntity), new[] {"idCol"}
            , new[] {"idCol"}, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade,
            Lazy = true)]
        public virtual LazyTestOne2OneEntity One2OneEntity { get; set; }
    }
}