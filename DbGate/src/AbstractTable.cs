using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate
{
    public abstract class AbstractTable : ITable
    {
        protected AbstractTable(string tableName, UpdateStrategy updateStrategy, VerifyOnWriteStrategy verifyOnWriteStrategy
            , DirtyCheckStrategy dirtyCheckStrategy)
        {
            TableName = tableName;
            UpdateStrategy = updateStrategy;
            VerifyOnWriteStrategy = verifyOnWriteStrategy;
            DirtyCheckStrategy = dirtyCheckStrategy;
        }

        protected AbstractTable(string tableName) : this(tableName,UpdateStrategy.Default
            ,VerifyOnWriteStrategy.Default,DirtyCheckStrategy.Default)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }

        public UpdateStrategy UpdateStrategy { get; set; }

        public VerifyOnWriteStrategy VerifyOnWriteStrategy { get; set; }

        public DirtyCheckStrategy DirtyCheckStrategy { get; set; }
    }
}
