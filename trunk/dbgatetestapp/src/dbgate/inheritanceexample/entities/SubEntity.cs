using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.inheritanceexample.entities
{
    [DbTableInfo("sub_entity")]
    public class SubEntity : MiddleEntity
    {
        [DbColumnInfo(DbColumnType.Varchar)]
        public string SubName { get; set; }

        public SubEntity()
        {
        }
    }
}
