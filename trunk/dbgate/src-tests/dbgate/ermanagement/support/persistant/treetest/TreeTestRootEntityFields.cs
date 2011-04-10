using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.treetest
{
    public class TreeTestRootEntityFields : AbstractManagedDbClass , ITreeTestRootEntity
    {
        public int IdCol { get; set; }
        public string Name { get; set; }
        public List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }
        public ITreeTestOne2OneEntity One2OneEntity { get; set; }

        public override Dictionary<Type, string> TableNames
        {
            get 
            {
                Dictionary<Type,string> map = new Dictionary<Type, string>();
                map.Add(typeof(TreeTestRootEntityFields),"tree_test_root");
                return map; 
            }
        }

        public override Dictionary<Type, ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type,ICollection<IField>> map = new Dictionary<Type, ICollection<IField>>();
                List<IField> dbColumns = new List<IField>();

                dbColumns.Add(new DefaultDbColumn("IdCol",true,false,DbColumnType.Integer));
                dbColumns.Add(new DefaultDbColumn("Name",DbColumnType.Varchar));
                dbColumns.Add(new DefaultDbRelation("One2ManyEntities","fk_root2one2manyent",typeof(TreeTestOne2ManyEntityFields)
                    ,new DbRelationColumnMapping[]{new DbRelationColumnMapping("idCol","idCol")}));
                dbColumns.Add(new DefaultDbRelation("One2OneEntity","fk_root2one2oneent", typeof(TreeTestOne2OneEntityFields)
                    ,new DbRelationColumnMapping[]{new DbRelationColumnMapping("idCol","idCol")}));

                map.Add(typeof(TreeTestRootEntityFields),dbColumns);
                return map;
            }
        }
    }
}