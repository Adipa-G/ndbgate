using System;
using System.Collections.Generic;

namespace DbGate.Support.Patch.PatchEmpty
{
    public class LeafEntitySubA : LeafEntity
    {
        public string SomeTextA { get; set; }

        public override Dictionary<Type, string> TableNames
        {
            get
            {
                Dictionary<Type, string> map = base.TableNames;
                map.Add(typeof (LeafEntitySubA), "leaf_entity_a");
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = base.FieldInfo;
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("SomeTextA", ColumnType.Varchar));

                map.Add(typeof (LeafEntitySubA), dbColumns);
                return map;
            }
        }
    }
}