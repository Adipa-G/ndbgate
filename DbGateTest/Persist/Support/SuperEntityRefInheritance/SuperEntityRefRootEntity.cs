using System.Collections.Generic;

namespace DbGate.Persist.Support.SuperEntityRefInheritance
{
    [TableInfo("super_entity_ref_test_root")]
    public class SuperEntityRefRootEntity : DefaultEntity
    {
        public SuperEntityRefRootEntity()
        {
            One2ManyEntities = new List<SuperEntityRefOne2ManyEntity>();
        }

        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar,Size = 100)]
        public virtual string Name { get; set; }

        [ForeignKeyInfo("fk_root2one2manyent",
            typeof(SuperEntityRefOne2ManyEntity),
            new[] { "idCol" },
            new[] {"idCol"},
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade,
            FetchStrategy = FetchStrategy.Eager)]
        public virtual ICollection<SuperEntityRefOne2ManyEntity> One2ManyEntities { get; set; }

        [ForeignKeyInfo("fk_root2one2oneent",
            typeof(SuperEntityRefOne2OneEntity),
            new[] { "idCol" },
            new[] {"idCol"},
            UpdateRule = ReferentialRuleType.Restrict,
            DeleteRule = ReferentialRuleType.Cascade,
            FetchStrategy = FetchStrategy.Eager)]
        public virtual SuperEntityRefOne2OneEntity One2OneEntity { get; set; }
    }
}