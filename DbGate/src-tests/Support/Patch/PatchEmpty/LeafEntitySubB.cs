using System;
using System.Collections.Generic;

namespace DbGate.Support.Patch.PatchEmpty
{
    public class LeafEntitySubB : LeafEntity
    {
        public string SomeTextB { get; set; }

        public override Dictionary<Type, string> TableNames
        {
            get
            {
                Dictionary<Type, string> map = base.TableNames;
                map.Add(typeof (LeafEntitySubB), "leaf_entity_b");
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = base.FieldInfo;
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("SomeTextB", ColumnType.Varchar));

                map.Add(typeof (LeafEntitySubB), dbColumns);
                return map;
            }
        }
    }
}