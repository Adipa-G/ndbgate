﻿using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestSubEntityAFields : InheritanceTestSuperEntityFields, IInheritanceTestSubEntityA
    {
        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                Dictionary<Type, ITable> map = base.TableInfo;
                map.Add(typeof (InheritanceTestSubEntityAFields), new DefaultTable("inheritance_test_suba"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = base.FieldInfo;
                ICollection<IField> dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("NameA", ColumnType.Varchar));

                map.Add(typeof (InheritanceTestSubEntityAFields), dbColumns);
                return map;
            }
        }

        #region IInheritanceTestSubEntityA Members

        public string NameA { get; set; }

        #endregion
    }
}