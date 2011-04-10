using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.simpleexample.entities
{
    [DbTableInfo("simple_entity")]
    public class SimpleEntity  : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true)]
        public int Id { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string Name { get; set; }

        public SimpleEntity()
        {
        }
    }
}
