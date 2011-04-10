using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.inheritanceexample.entities
{
    [DbTableInfo("middle_entity")]
    public class MiddleEntity : SuperEntity
    {
        [DbColumnInfo(DbColumnType.Varchar)]
        public string MiddleName { get; set; }

        public MiddleEntity()
        {
        }
    }
}
