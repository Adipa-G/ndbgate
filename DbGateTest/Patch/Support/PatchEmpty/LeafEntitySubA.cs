using System;
using System.Collections.Generic;

namespace DbGate.Patch.Support.PatchEmpty
{
    public class LeafEntitySubA : LeafEntity
    {
        public string SomeTextA { get; set; }

        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = base.TableInfo;
                map.Add(typeof (LeafEntitySubA), new DefaultTable("leaf_entity_a"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = base.FieldInfo;
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("SomeTextA", ColumnType.Varchar));

                map.Add(typeof (LeafEntitySubA), dbColumns);
                return map;
            }
        }
    }
}