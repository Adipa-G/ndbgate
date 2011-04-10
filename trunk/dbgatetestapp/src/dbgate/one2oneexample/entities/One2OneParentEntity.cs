using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2oneexample.entities
{
    [DbTableInfo("parent_entity")]
    public class One2OneParentEntity : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        [ForeignKeyInfo("parent2childA", typeof(One2OneChildEntityA), new string[] { "id" }
            , new string[] { "parentId" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        [ForeignKeyInfo("parent2childB", typeof(One2OneChildEntityB), new string[] { "id" }
            , new string[] { "parentId" }, UpdateRule = ReferentialRuleType.Restrict, DeleteRule = ReferentialRuleType.Cascade)]
        public One2OneChildEntity ChildEntity { get; set; }

        public One2OneParentEntity()
        {
        }
    }
}
