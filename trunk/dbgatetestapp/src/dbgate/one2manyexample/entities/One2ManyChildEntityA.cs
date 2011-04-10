using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.one2manyexample.entities
{
    [DbTableInfo("child_entity_a")]
    public class One2ManyChildEntityA : One2ManyChildEntity
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int ParentId { get; set; }

        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int IndexNo { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public override string Name { get; set; }
    }
}

