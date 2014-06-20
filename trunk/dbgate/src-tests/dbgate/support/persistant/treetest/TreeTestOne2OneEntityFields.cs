using System;
using System.Collections.Generic;
using dbgate.ermanagement;

namespace dbgate.support.persistant.treetest
{
    public class TreeTestOne2OneEntityFields : AbstractManagedEntity , ITreeTestOne2OneEntity
    {
        public int IdCol { get; set; }
        public string Name { get; set; }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = new Dictionary<Type, ICollection<IField>>();
                List<IField> dbColumns = new List<IField>();

                dbColumns.Add(new DefaultColumn("IdCol", true, false, ColumnType.Integer));
                dbColumns.Add(new DefaultColumn("Name", ColumnType.Varchar));

                map.Add(typeof(TreeTestOne2OneEntityFields), dbColumns);
                return map;
            }
        }

        public override Dictionary<Type, string> TableNames
        {
            get
            {
                Dictionary<Type, string> map = new Dictionary<Type, string>();
                map.Add(typeof(TreeTestOne2OneEntityFields), "tree_test_one2one");
                return map;
            }
        }
    }
}