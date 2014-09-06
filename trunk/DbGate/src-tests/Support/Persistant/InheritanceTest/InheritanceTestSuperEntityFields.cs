using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.InheritanceTest
{
    public class InheritanceTestSuperEntityFields : AbstractManagedEntity, IInheritanceTestSuperEntity
    {
        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = new Dictionary<Type, ITable>();
                map.Add(typeof (InheritanceTestSuperEntityFields), new DefaultTable("inheritance_test_super"));
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                var map = new Dictionary<Type, ICollection<IField>>();
                var dbColumns = new List<IField>();

                var idCol = new DefaultColumn("IdCol", true, false, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                dbColumns.Add(idCol);
                dbColumns.Add(new DefaultColumn("Name", ColumnType.Varchar));

                map.Add(typeof (InheritanceTestSuperEntityFields), dbColumns);
                return map;
            }
        }

        #region IInheritanceTestSuperEntity Members

        public int IdCol { get; set; }
        public string Name { get; set; }

        #endregion
    }
}