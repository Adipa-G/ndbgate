using System;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.dbmm.defaultmm;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.dbmm.myqlmm
{
	public class MySqlMetaManipulate : DefaultMetaManipulate
    {
        public MySqlMetaManipulate(IDbLayer dbLayer,IDbGateConfig config) : base(dbLayer,config)
        {
        }

        public override bool Equals(IMetaItem iMetaItemA, IMetaItem iMetaItemB)
        {
            if (iMetaItemA.ItemType == MetaItemType.PrimaryKey
                    && iMetaItemA.ItemType == iMetaItemB.ItemType)
            {
                MetaPrimaryKey primaryKeyA = (MetaPrimaryKey) iMetaItemA;
                MetaPrimaryKey primaryKeyB = (MetaPrimaryKey) iMetaItemB;

                if (primaryKeyA.ColumnNames.Count != primaryKeyB.ColumnNames.Count)
                {
                    return false;
                }
                foreach (string columnA in primaryKeyA.ColumnNames)
                {
                    bool found = false;
                    foreach (string columnB in primaryKeyB.ColumnNames)
                    {
                        if (columnA.Equals(columnB,StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
                return true;
            }
            return base.Equals(iMetaItemA, iMetaItemB);
        }
    }
}
