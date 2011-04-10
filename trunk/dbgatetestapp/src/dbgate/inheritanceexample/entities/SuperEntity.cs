using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.inheritanceexample.entities
{
    [DbTableInfo("super_entity")]
    public class SuperEntity : DefaultServerDbClass
    {
        [DbColumnInfo(DbColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int Id { get; set; }

        [DbColumnInfo(DbColumnType.Varchar)]
        public string SuperName { get; set; }

        public SuperEntity()
        {
        }
    }
}
