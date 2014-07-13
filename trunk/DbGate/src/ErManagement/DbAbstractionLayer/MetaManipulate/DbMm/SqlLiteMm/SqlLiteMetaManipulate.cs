using System.Data;
using System.Text;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.DefaultMm;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Mappings;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.SqlLiteMm
{
    public class SqlLiteMetaManipulate : DefaultMetaManipulate
    {
        public SqlLiteMetaManipulate(IDbLayer dbLayer, IDbGateConfig config) : base(dbLayer, config)
        {
        }

        protected override void FillDataMappings(ITransaction tx)
        {
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("INTEGER", ColumnType.Integer, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("BOOL", ColumnType.Boolean, "true"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("FLOAT", ColumnType.Float, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("CHAR", ColumnType.Char, "' '"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("DATE", ColumnType.Date, "1981/10/12"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("DOUBLE", ColumnType.Double, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("BIGINT", ColumnType.Long, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("TIMESTAMP", ColumnType.Timestamp, "1981/10/12"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("VARCHAR", ColumnType.Varchar, "''"));
        }

        protected override void ExtractPrimaryKeyData(ITransaction tx, MetaTable table)
        {
        }

        protected override void ExtractForeignKeyData(ITransaction tx, MetaTable table)
        {
        }

        protected override string CreateCreateTableQuery(MetaComparisonTableGroup tableGroup)
        {
            var metaTable = (MetaTable) tableGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" ( ");
            bool first = true;

            foreach (MetaColumn metaColumn in metaTable.Columns)
            {
                if (!first)
                {
                    sb.Append(" ,\n ");
                }
                else
                {
                    first = false;
                }
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
                sb.Append(" ");
                sb.Append(metaColumn.Null ? "" : "NOT NULL");
                sb.Append(" ");
            }

            if (tableGroup.PrimaryKey != null && tableGroup.PrimaryKey.RequiredItem != null)
            {
                var primaryKey = (MetaPrimaryKey) tableGroup.PrimaryKey.RequiredItem;
                sb.Append(",");
                sb.Append("PRIMARY KEY(");

                bool mapFirst = true;
                foreach (string colName in primaryKey.ColumnNames)
                {
                    if (!mapFirst)
                    {
                        sb.Append(",");
                    }
                    sb.Append(colName);
                    mapFirst = false;
                }
                sb.Append(") ");
            }

            foreach (MetaComparisonForeignKeyGroup foreignKeyGroup in tableGroup.ForeignKeys)
            {
                var requiredKey = (MetaForeignKey) foreignKeyGroup.RequiredItem;
                sb.Append(",");
                sb.Append("FOREIGN KEY(");

                bool mapFirst = true;
                foreach (MetaForeignKeyColumnMapping mapping in requiredKey.ColumnMappings)
                {
                    if (!mapFirst)
                    {
                        sb.Append(",");
                    }
                    sb.Append(mapping.FromColumn);
                    mapFirst = false;
                }
                sb.Append(") ");
                sb.Append("REFERENCES ");
                sb.Append(requiredKey.ToTable);
                sb.Append(" (");

                mapFirst = true;
                foreach (MetaForeignKeyColumnMapping mapping in requiredKey.ColumnMappings)
                {
                    if (!mapFirst)
                    {
                        sb.Append(",");
                    }
                    sb.Append(mapping.ToColumn);
                    mapFirst = false;
                }
                sb.Append(") ");
            }
            sb.Append(")");
            return sb.ToString();
        }

        protected override string CreateCreateForeginKeyQuery(MetaComparisonTableGroup tableGroup,
                                                              MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            return null;
        }

        protected override string CreateDropForeginKeyQuery(MetaComparisonTableGroup tableGroup,
                                                            MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            return null;
        }

        protected override string CreateCreatePrimaryKeyQuery(MetaComparisonTableGroup tableGroup,
                                                              MetaComparisonPrimaryKeyGroup primaryKeyGroup)
        {
            return null;
        }

        protected override string CreateDropPrimaryKeyQuery(MetaComparisonTableGroup tableGroup,
                                                            MetaComparisonPrimaryKeyGroup primaryKeyGroup)
        {
            return null;
        }

        protected override string CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup,
                                                         MetaComparisonColumnGroup columnGroup)
        {
            //sql lite does not support this
            return null;
        }

        protected override string CreateDropColumnQuery(MetaComparisonTableGroup tableGroup,
                                                        MetaComparisonColumnGroup columnGroup)
        {
            //sql lite does not support this
            return null;
        }
    }
}