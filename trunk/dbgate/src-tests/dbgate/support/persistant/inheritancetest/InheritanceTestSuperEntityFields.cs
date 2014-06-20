using System;
using System.Collections.Generic;
using dbgate.ermanagement;

namespace dbgate.support.persistant.inheritancetest
{
    public class InheritanceTestSuperEntityFields : AbstractManagedEntity ,IInheritanceTestSuperEntity
    {
        public int IdCol { get; set; }
        public string Name { get; set; }

        public override Dictionary<Type,string> TableNames
        {
            get
            {
                Dictionary<Type, string> map = new Dictionary<Type, string>();
                map.Add(typeof(InheritanceTestSuperEntityFields),"inheritance_test_super");
                return map;
            }
        }

        public override Dictionary<Type,ICollection<IField>> FieldInfo
        {
            get
            {
                Dictionary<Type, ICollection<IField>> map = new Dictionary<Type, ICollection<IField>>();
                List<IField> dbColumns = new List<IField>();

                DefaultColumn idCol = new DefaultColumn("IdCol", true, false, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                dbColumns.Add(idCol);
                dbColumns.Add(new DefaultColumn("Name", ColumnType.Varchar));

                map.Add(typeof(InheritanceTestSuperEntityFields),dbColumns);
                return map;
            }
        }
    }
}