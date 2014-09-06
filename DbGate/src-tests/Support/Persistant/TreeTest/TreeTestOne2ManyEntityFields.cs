using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.TreeTest
{
    public class TreeTestOne2ManyEntityFields : AbstractManagedEntity, ITreeTestOne2ManyEntity
    {
        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = new Dictionary<Type, ITable>();
                map.Add(typeof (TreeTestOne2ManyEntityFields), new DefaultTable("tree_test_one2many"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = new Dictionary<Type, ICollection<IField>>();
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("IdCol", true, false, ColumnType.Integer));
                dbColumns.Add(new DefaultColumn("IndexNo", true, false, ColumnType.Integer));
                dbColumns.Add(new DefaultColumn("Name", ColumnType.Varchar));

                map.Add(typeof (TreeTestOne2ManyEntityFields), dbColumns);
                return map;
            }
        }

        #region ITreeTestOne2ManyEntity Members

        public int IdCol { get; set; }
        public int IndexNo { get; set; }
        public string Name { get; set; }

        #endregion
    }
}