using dbgate;
using dbgate.ermanagement;

namespace dbgatetestapp.dbgate.inheritanceexample.entities
{
    [TableInfo("sub_entity")]
    public class SubEntity : MiddleEntity
    {
        [ColumnInfo(ColumnType.Varchar)]
        public string SubName { get; set; }

        public SubEntity()
        {
        }
    }
}
