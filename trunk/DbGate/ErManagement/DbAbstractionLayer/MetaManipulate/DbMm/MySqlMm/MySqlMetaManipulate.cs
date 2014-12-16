using System;
using System.Text;
using DbGate;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.DefaultMm;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Mappings;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.MySqlMm
{
    public class MySqlMetaManipulate : DefaultMetaManipulate
    {
        public MySqlMetaManipulate(IDbLayer dbLayer, IDbGateConfig config) : base(dbLayer, config)
        {
        }

        protected override void FillReferentialRuleMappings(ITransaction tx)
	    {
		    ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Cascade, "0"));
		    ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Restrict, "3"));
	    }

        protected override string CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup)
        {
            var metaTable = (MetaTable)tableGroup.RequiredItem;
            var metaColumn = (MetaColumn)columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" MODIFY ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");
            if (metaColumn.ColumnType == ColumnType.Char
                    || metaColumn.ColumnType == ColumnType.Varchar)
            {
                sb.Append(MapColumnTypeToTypeName(metaColumn.ColumnType));
                sb.Append("(");
                sb.Append(metaColumn.Size);
                sb.Append(")");
            }
            else
            {
                sb.Append(MapColumnTypeToTypeName(metaColumn.ColumnType));
            }

            sb.Append(" DEFAULT ");
            if (!metaColumn.Null)
            {
                String defaultValue = GetDefaultValueForType(metaColumn.ColumnType);
                if (defaultValue != null)
                {
                    sb.Append(defaultValue);
                }
            }
            else
            {
                sb.Append("NULL");
            }

            sb.Append(" ");
            sb.Append(metaColumn.Null ? "" : "NOT NULL");
            sb.Append(" ");

            return sb.ToString();
        }

	    protected override string CreateDropPrimaryKeyQuery(MetaComparisonTableGroup tableGroup
            , MetaComparisonPrimaryKeyGroup primaryKeyGroup)
	    {
		    var requiredTable = (MetaTable)tableGroup.ExistingItem;
		    var primaryKey = (MetaColumn)primaryKeyGroup.ExistingItem;

		    var sb = new StringBuilder();
		    sb.Append("ALTER TABLE ");
		    sb.Append(requiredTable.Name);
		    sb.Append(" DROP PRIMARY KEY");
		    return sb.ToString();
	    }

        protected override string CreateDropForeignKeyQuery(MetaComparisonTableGroup tableGroup
            ,MetaComparisonForeignKeyGroup foreignKeyGroup)
	    {
		    var requiredTable = (MetaTable)tableGroup.ExistingItem;
            var metaForeignKey = (MetaColumn)foreignKeyGroup.RequiredItem;

		    var sb = new StringBuilder();
		    sb.Append("ALTER TABLE ");
		    sb.Append(requiredTable.Name);
		    sb.Append(" DROP FOREIGN KEY ");
		    sb.Append(metaForeignKey.Name);
		    return sb.ToString();
	    }

        public override bool Equals(IMetaItem iMetaItemA, IMetaItem iMetaItemB)
        {
            if (iMetaItemA.ItemType == MetaItemType.PrimaryKey
                && iMetaItemA.ItemType == iMetaItemB.ItemType)
            {
                var primaryKeyA = (MetaPrimaryKey) iMetaItemA;
                var primaryKeyB = (MetaPrimaryKey) iMetaItemB;

                if (primaryKeyA.ColumnNames.Count != primaryKeyB.ColumnNames.Count)
                {
                    return false;
                }
                foreach (string columnA in primaryKeyA.ColumnNames)
                {
                    bool found = false;
                    foreach (string columnB in primaryKeyB.ColumnNames)
                    {
                        if (columnA.Equals(columnB, StringComparison.OrdinalIgnoreCase))
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