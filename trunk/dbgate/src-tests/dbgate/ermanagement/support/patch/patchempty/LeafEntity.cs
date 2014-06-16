using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.patch.patchempty
{
	public class LeafEntity  : AbstractManagedEntity
	{
	    public int IdCol { get; set; }
		public int IndexNo { get; set; }
		public string SomeText { get; set; }

		public override Dictionary<Type,string> TableNames
		{
            get
            {
                Dictionary<Type,string> map = new Dictionary<Type, String>();
			    map.Add(typeof(LeafEntity),"leaf_entity");
			    return map;
            }
		}

		public override Dictionary<Type,ICollection<IField>> FieldInfo
		{
            get
            {
                Dictionary<Type, ICollection<IField>> map = new Dictionary<Type, ICollection<IField>>();
                List<IField> dbColumns = new List<IField>();

                IColumn idCol = new DefaultColumn("IdCol", true, ColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                dbColumns.Add(idCol);

                IColumn indexCol = new DefaultColumn("IndexNo", true, ColumnType.Integer);
                indexCol.SubClassCommonColumn = true;
                dbColumns.Add(indexCol);

                dbColumns.Add(new DefaultColumn("SomeText", ColumnType.Varchar));

                map.Add(typeof(LeafEntity),dbColumns);
                return map;
            }
		}
	}
}
