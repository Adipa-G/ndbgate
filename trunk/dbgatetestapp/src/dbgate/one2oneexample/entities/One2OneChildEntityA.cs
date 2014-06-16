using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2oneexample.entities
{
    [TableInfo("child_entity_a")]
    public class One2OneChildEntityA : One2OneChildEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public override string Name { get; set; }
    }
}
