using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures
{
    public class MetaPrimaryKey : AbstractMetaItem
    {
        public MetaPrimaryKey()
        {
            ItemType = MetaItemType.PrimaryKey;
            ColumnNames = new List<string>();
        }

        public ICollection<string> ColumnNames { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            if (!base.Equals(o)) return false;

            MetaPrimaryKey that = (MetaPrimaryKey) o;

            bool foundMatch = false;
            foreach (string thisColumn in ColumnNames)
            {
                foreach (string thatColumn in that.ColumnNames)
                {
                    if (thisColumn.Equals(thatColumn,StringComparison.OrdinalIgnoreCase))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
