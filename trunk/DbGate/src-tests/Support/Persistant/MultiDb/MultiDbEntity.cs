using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate.src_tests.Support.Persistant.MultiDb
{
    [TableInfo("multi_db_test_root")]
    public class MultiDbEntity : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public virtual int IdCol { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public virtual string Name { get; set; }
    }
}
