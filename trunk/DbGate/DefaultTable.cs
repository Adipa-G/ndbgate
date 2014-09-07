using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate
{
    public class DefaultTable : AbstractTable
    {
        public DefaultTable(string tableName, UpdateStrategy updateStrategy
            , VerifyOnWriteStrategy verifyOnWriteStrategy, DirtyCheckStrategy dirtyCheckStrategy) 
            : base(tableName, updateStrategy, verifyOnWriteStrategy, dirtyCheckStrategy)
        {
        }

        public DefaultTable(string tableName) : base(tableName)
        {
        }
    }
}
