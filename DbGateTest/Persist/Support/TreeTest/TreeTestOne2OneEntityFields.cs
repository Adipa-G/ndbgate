﻿using System;
using System.Collections.Generic;

namespace DbGate.Persist.Support.TreeTest
{
    public class TreeTestOne2OneEntityFields : AbstractManagedEntity, ITreeTestOne2OneEntity
    {
        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = new Dictionary<Type, ICollection<IField>>();
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("IdCol", true, false, ColumnType.Integer));
                dbColumns.Add(new DefaultColumn("Name", ColumnType.Varchar));

                map.Add(typeof (TreeTestOne2OneEntityFields), dbColumns);
                return map;
            }
        }

        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = new Dictionary<Type, ITable>();
                map.Add(typeof (TreeTestOne2OneEntityFields), new DefaultTable("tree_test_one2one"));
                return map;
            }
        }

        #region ITreeTestOne2OneEntity Members

        public int IdCol { get; set; }
        public string Name { get; set; }

        #endregion
    }
}