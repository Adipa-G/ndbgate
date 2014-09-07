using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate.Caches.Impl
{
    public class EntityRelationColumnInfo
    {
        public EntityRelationColumnInfo(IColumn column, IRelation relation, RelationColumnMapping mapping)
        {
            Column = column;
            Relation = relation;
            Mapping = mapping;
        }

        public IColumn Column { get; private set; }
        public IRelation Relation { get; private set; }
        public RelationColumnMapping Mapping { get; private set; }
    }
}
