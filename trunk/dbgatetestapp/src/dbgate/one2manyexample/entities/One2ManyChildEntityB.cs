using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2manyexample.entities
{
    [TableInfo("child_entity_b")]
    public class One2ManyChildEntityB : One2ManyChildEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public override string Name { get; set; }
    }
}