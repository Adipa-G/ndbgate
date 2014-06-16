using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.inheritanceexample.entities
{
    [TableInfo("middle_entity")]
    public class MiddleEntity : SuperEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string MiddleName { get; set; }

        public MiddleEntity()
        {
        }
    }
}
