using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2oneexample.entities
{
    [DbTableInfo("child_entity_b")]
    public class One2OneChildEntityB : One2OneChildEntity
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public override string Name { get; set; }
    }
}
