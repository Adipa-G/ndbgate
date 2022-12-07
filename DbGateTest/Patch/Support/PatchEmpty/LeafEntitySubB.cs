using System;
using System.Collections.Generic;

namespace DbGate.Patch.Support.PatchEmpty
{
    public class LeafEntitySubB : LeafEntity
    {
        public string SomeTextB { get; set; }

        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = base.TableInfo;
                map.Add(typeof (LeafEntitySubB), new DefaultTable("leaf_entity_b"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = base.FieldInfo;
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("SomeTextB", ColumnType.Varchar));

                map.Add(typeof (LeafEntitySubB), dbColumns);
                return map;
            }
        }
    }
}