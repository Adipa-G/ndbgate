﻿using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestSubEntityBFields : InheritanceTestSuperEntityFields, IInheritanceTestSubEntityB
    {
        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                Dictionary<Type, ITable> map = base.TableInfo;
                map.Add(typeof (InheritanceTestSubEntityBFields), new DefaultTable("inheritance_test_subb"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = base.FieldInfo;
                var dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("NameB", ColumnType.Varchar));

                map.Add(typeof (InheritanceTestSubEntityBFields), dbColumns);
                return map;
            }
        }

        #region IInheritanceTestSubEntityB Members

        public string NameB { get; set; }

        #endregion
    }
}