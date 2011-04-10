using System.Collections.Generic;
using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2manyexample.entities
{
    [DbTableInfo("parent_entity")]
    public class One2ManyParentEntity : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("parent2childA", typeof(One2ManyChildEntityA), new string[] { "id" }
            , new string[] { "parentId" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        [ForeignKeyInfo("parent2childB", typeof(One2ManyChildEntityB), new string[] { "id" }
            , new string[] { "parentId" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public ICollection<One2ManyChildEntity> ChildEntities { get; set; }

        public One2ManyParentEntity()
        {
            ChildEntities = new List<One2ManyChildEntity>();
        }
    }
}
