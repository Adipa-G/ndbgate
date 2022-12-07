using System;
using System.Collections.Generic;

namespace DbGate.Exception.Support
{
    [TableInfo("exception_test_root")]
    public class EntityWithAllWrong : AbstractManagedEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true)]
        public int IdCol
        {
            get => throw new System.Exception("cant' get");
            set => throw new System.Exception("cant' set");
        }
 		
 		public EntityWithAllWrong(int id)
 		{
 		    
 		}

        public override Dictionary<Type, ICollection<IField>> FieldInfo => null;

        public override Dictionary<Type, ITable> TableInfo => null;
    }
}
