using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.support.patch.patchempty
{
	public class LeafEntity  : AbstractManagedDbClass
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

                IDbColumn idCol = new DefaultDbColumn("IdCol", true, DbColumnType.Integer);
                idCol.SubClassCommonColumn = true;
                dbColumns.Add(idCol);

                IDbColumn indexCol = new DefaultDbColumn("IndexNo", true, DbColumnType.Integer);
                indexCol.SubClassCommonColumn = true;
                dbColumns.Add(indexCol);

                dbColumns.Add(new DefaultDbColumn("SomeText", DbColumnType.Varchar));

                map.Add(typeof(LeafEntity),dbColumns);
                return map;
            }
		}
	}
}
