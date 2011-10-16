using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.patch.patchempty
{
    public class LeafEntitySubB : LeafEntity
    {
        public string SomeTextB { get; set; }

        public override Dictionary<Type, string> TableNames
        {
            get
            {
                Dictionary<Type, string> map = base.TableNames;
                map.Add(typeof(LeafEntitySubB), "leaf_entity_b");
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = base.FieldInfo;
                List<IField> dbColumns = new List<IField>();

                dbColumns.Add(new DefaultDbColumn("SomeTextB", DbColumnType.Varchar));

                map.Add(typeof(LeafEntitySubB), dbColumns);
                return map;
            }
        }
    }
}