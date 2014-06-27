using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestSubEntityAFields : InheritanceTestSuperEntityFields, IInheritanceTestSubEntityA
    {
        public override Dictionary<Type, string> TableNames
        {
            get
            {
                Dictionary<Type, String> map = base.TableNames;
                map.Add(typeof (InheritanceTestSubEntityAFields), "inheritance_test_suba");
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