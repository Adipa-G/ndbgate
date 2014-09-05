using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate.Support.Persistant.NonIdentifyingRelationWithoutColumn
{
    [TableInfo("relation_test_currency")]
    public class Currency : DefaultEntity
    {
        [ColumnInfo((ColumnType.Integer), Key = true)]
        public int CurrencyId { get; set; }

        [ColumnInfo((ColumnType.Varchar))]
        public string Code { get; set; }
    }
}
