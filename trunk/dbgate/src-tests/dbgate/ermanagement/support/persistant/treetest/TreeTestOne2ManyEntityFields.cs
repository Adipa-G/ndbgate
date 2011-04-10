using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.treetest
{
    public class TreeTestOne2ManyEntityFields : AbstractManagedDbClass , ITreeTestOne2ManyEntity
    {
        public int IdCol { get; set; }
        public int IndexNo { get; set; }
        public string Name { get; set; }

        public override Dictionary<Type, string> TableNames
        {
            get
            {
                Dictionary<Type, string> map = new Dictionary<Type, string>();
                map.Add(typeof(TreeTestOne2ManyEntityFields) , "tree_test_one2many");
                return map;
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = new Dictionary<Type, ICollection<IField>>();
                List<IField> dbColumns = new List<IField>();

                dbColumns.Add(new DefaultDbColumn("IdCol", true, false, DbColumnType.Integer));
                dbColumns.Add(new DefaultDbColumn("IndexNo", true, false, DbColumnType.Integer));
                dbColumns.Add(new DefaultDbColumn("Name", DbColumnType.Varchar));

                map.Add(typeof(TreeTestOne2ManyEntityFields), dbColumns);
                return map;
            }
        }
    }
}