using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2oneexample.entities
{
    [DbTableInfo("child_entity_a")]
    public class One2OneChildEntityA : One2OneChildEntity
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public override string Name { get; set; }
    }
}
