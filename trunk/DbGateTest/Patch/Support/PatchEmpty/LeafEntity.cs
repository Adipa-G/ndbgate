using System;
using System.Collections.Generic;

namespace DbGate.Patch.Support.PatchEmpty
{
    public class LeafEntity : AbstractManagedEntity
    {
        public int IdCol { get; set; }
        public int IndexNo { get; set; }
        public string SomeText { get; set; }

        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = new Dictionary<Type, ITable>();
                map.Add(typeof (LeafEntity), new DefaultTable("leaf_entity"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = new Dictionary<Type, ICollection<IField>>();
                var dbColumns = new List<IField>();

                IColumn idCol = new DefaultColumn("IdCol", true, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                dbColumns.Add(idCol);

                IColumn indexCol = new DefaultColumn("IndexNo", true, ColumnType.Integer);
                indexCol.SubClassCommonColumn = true;
                dbColumns.Add(indexCol);

                dbColumns.Add(new DefaultColumn("SomeText", ColumnType.Varchar));

                map.Add(typeof (LeafEntity), dbColumns);
                return map;
            }
        }
    }
}