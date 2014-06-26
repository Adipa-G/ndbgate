using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.dbabstractionlayer.metamanipulate.datastructures
{
    public class MetaForeignKey : AbstractMetaItem
    {
        public MetaForeignKey()
        {
            ItemType = MetaItemType.ForeignKey;
            ColumnMappings = new List<MetaForeignKeyColumnMapping>();
        }

        public string ToTable { get; set; }

        public ReferentialRuleType UpdateRule { get; set; }

        public ReferentialRuleType DeleteRule { get; set; }

        public ICollection<MetaForeignKeyColumnMapping> ColumnMappings { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            if (!base.Equals(o)) return false;

            MetaForeignKey that = (MetaForeignKey) o;

            bool foundMatch = false;
            foreach (MetaForeignKeyColumnMapping thisMapping in ColumnMappings)
            {
                foreach (MetaForeignKeyColumnMapping thatMapping in that.ColumnMappings)
                {
                    if (thisMapping.Equals(thatMapping))
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

            if (DeleteRule != that.DeleteRule) return false;
            if (!ToTable.Equals(that.ToTable,StringComparison.OrdinalIgnoreCase)) return false;
            if (UpdateRule != that.UpdateRule) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
