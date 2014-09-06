using System;
using System.Collections.Generic;

namespace DbGate.Support.Persistant.TreeTest
{
    public class TreeTestRootEntityFields : AbstractManagedEntity, ITreeTestRootEntity
    {
        public override Dictionary<Type, ITable> TableInfo
        {
            get
            {
                var map = new Dictionary<Type, ITable>();
                map.Add(typeof (TreeTestRootEntityFields), new DefaultTable("tree_test_root"));
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
                dbColumns.Add(new DefaultColumn("Name", ColumnType.Varchar));
                dbColumns.Add(new DefaultRelation("One2ManyEntities", "fk_root2one2manyent",
                                                  typeof (TreeTestOne2ManyEntityFields)
                                                  , new[] {new RelationColumnMapping("idCol", "idCol")}));
                dbColumns.Add(new DefaultRelation("One2OneEntity", "fk_root2one2oneent",
                                                  typeof (TreeTestOne2OneEntityFields)
                                                  , new[] {new RelationColumnMapping("idCol", "idCol")}));

                map.Add(typeof (TreeTestRootEntityFields), dbColumns);
                return map;
            }
        }

        #region ITreeTestRootEntity Members

        public int IdCol { get; set; }
        public string Name { get; set; }
        public List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }
        public ITreeTestOne2OneEntity One2OneEntity { get; set; }

        #endregion
    }
}