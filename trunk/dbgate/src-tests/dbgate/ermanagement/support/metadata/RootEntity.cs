using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.metadata
{
    public class RootEntity : AbstractManagedDbClass
    {
        public int IdCol { get; set; }
        public long LongNotNull{ get; set; }
        public long LongNull{ get; set; }
        public bool BooleanNotNull{ get; set; }
        public bool? BooleanNull{ get; set; }
        public char CharNotNull{ get; set; }
        public char? CharNull{ get; set; }
        public int IntNotNull{ get; set; }
        public int? IntNull{ get; set; }
        public DateTime? DateNotNull{ get; set; }
        public DateTime? DateNull{ get; set; }
        public double DoubleNotNull{ get; set; }
        public double? DoubleNull{ get; set; }
        public float FloatNotNull{ get; set; }
        public float? FloatNull{ get; set; }
        public DateTime? TimestampNotNull{ get; set; }
        public DateTime? TimestampNull{ get; set; }
        public String VarcharNotNull{ get; set; }
        public String VarcharNull{ get; set; }

        [ForeignKeyInfo("fk_root2leafa", typeof(LeafEntitySubA),new[]{"idcol"},new[]{"idcol"})]
        [ForeignKeyInfo("fk_root2leafb",typeof(LeafEntitySubB),new[]{"idcol"},new[]{"idcol"})]
        public ICollection<LeafEntity> LeafEntities { get; set; }

        public RootEntity()
        {
            LeafEntities = new List<LeafEntity>();
        }

        public override Dictionary<Type,String> TableNames
        {
            get
            {
                Dictionary<Type, String> map = new Dictionary<Type, String>();
                map.Add(typeof(RootEntity), "root_entity");
                return map;
            }
        }

        public override Dictionary<Type,ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = new Dictionary<Type, ICollection<IField>>();
                List<IField> dbColumns = new List<IField>();

                dbColumns.Add(new DefaultDbColumn("IdCol", true, DbColumnType.Integer));
                dbColumns.Add(new DefaultDbColumn("LongNotNull", DbColumnType.Long));
                dbColumns.Add(new DefaultDbColumn("LongNull", DbColumnType.Long, true));
                dbColumns.Add(new DefaultDbColumn("BooleanNotNull", DbColumnType.Boolean));
                dbColumns.Add(new DefaultDbColumn("BooleanNull", DbColumnType.Boolean, true));
                dbColumns.Add(new DefaultDbColumn("CharNotNull", DbColumnType.Char));
                dbColumns.Add(new DefaultDbColumn("CharNull", DbColumnType.Char, true));
                dbColumns.Add(new DefaultDbColumn("IntNotNull", DbColumnType.Integer));
                dbColumns.Add(new DefaultDbColumn("IntNull", DbColumnType.Integer, true));
                dbColumns.Add(new DefaultDbColumn("DateNotNull", DbColumnType.Date));
                dbColumns.Add(new DefaultDbColumn("DateNull", DbColumnType.Date, true));
                dbColumns.Add(new DefaultDbColumn("DoubleNotNull", DbColumnType.Double));
                dbColumns.Add(new DefaultDbColumn("DoubleNull", DbColumnType.Double, true));
                dbColumns.Add(new DefaultDbColumn("FloatNotNull", DbColumnType.Float));
                dbColumns.Add(new DefaultDbColumn("FloatNull", DbColumnType.Float, true));
                dbColumns.Add(new DefaultDbColumn("TimestampNotNull", DbColumnType.Timestamp));
                dbColumns.Add(new DefaultDbColumn("TimestampNull", DbColumnType.Timestamp, true));
                dbColumns.Add(new DefaultDbColumn("VarcharNotNull", DbColumnType.Varchar));
                dbColumns.Add(new DefaultDbColumn("VarcharNull", DbColumnType.Varchar, true));

                map.Add(typeof(RootEntity), dbColumns);
                return map;    
            }
        }
    }
}