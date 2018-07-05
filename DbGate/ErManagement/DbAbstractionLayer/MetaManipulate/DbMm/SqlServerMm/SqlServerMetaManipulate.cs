using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.DefaultMm;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Mappings;
using DbGate.Exceptions;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.SqlServerMm
{
    public class SqlServerMetaManipulate : DefaultMetaManipulate
    {
        public SqlServerMetaManipulate(IDbLayer dbLayer, IDbGateConfig config) : base(dbLayer, config)
        {
        }

        protected override void FillReferentialRuleMappings(ITransaction tx)
        {
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Unknown, "0"));
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Cascade, "1"));
            ReferentialRuleTypeMapItems.Add(new ReferentialRuleTypeMapItem(ReferentialRuleType.Restrict, "2"));
        }

        protected override void FillDataMappings(ITransaction tx)
        {
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("INT", ColumnType.Integer, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("BIT", ColumnType.Boolean, "true"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("FLOAT", ColumnType.Float, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("CHAR", ColumnType.Char, "' '"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("DATE", ColumnType.Date, "1981/10/12"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("FLOAT", ColumnType.Double, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("BIGINT", ColumnType.Long, "0"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("TIMESTAMP", ColumnType.Timestamp, "1981/10/12"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("VARCHAR", ColumnType.Varchar, "''"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("NVARCHAR", ColumnType.Varchar, "''"));
            ColumnTypeMapItems.Add(new ColumnTypeMapItem("UNIQUEIDENTIFIER", ColumnType.Guid, Guid.Empty.ToString()));
        }

        protected override ICollection<MetaTable> ExtractTableData(ITransaction tx)
        {
            var metaItems = new List<MetaTable>();
            try
            {
                var cmd = tx.CreateCommand();
                cmd.CommandText = "SELECT name FROM sys.Tables;";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var table = new MetaTable();
                        metaItems.Add(table);
                        table.Name = reader["name"].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    "Exception occured while trying to read table information", e);
                throw new DBPatchingException(e.Message, e);
            }
            return metaItems;
        }

        protected override void ExtractColumnData(ITransaction tx, MetaTable table)
        {
            try
            {
                var cmd = tx.CreateCommand();
                cmd.CommandText = $"select * from {table.Name};";

                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    var columnTable = reader.GetSchemaTable();
                    if (columnTable == null)
                    {
                        Logger.GetLogger(Config.LoggerName).Fatal(
                            string.Format("Unable to read the list of columns in table {0}", table.Name));
                        return;
                    }
                    foreach (DataRow columnRow in columnTable.Rows)
                    {
                        var column = new MetaColumn();
                        table.Columns.Add(column);
                        column.Name = columnRow["ColumnName"].ToString();
                        column.Size = (short)columnRow["NumericPrecision"];
                        if (column.Size == 0)
                        {
                            column.Size = 255;
                        }
                        column.ColumnType = MapColumnTypeNameToType(columnRow["DataTypeName"].ToString());
                        column.Null = (bool)columnRow["AllowDBNull"];
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    string.Format("Exception occured while trying to read column information in table {0}", table.Name),
                    e);
                throw new DBPatchingException(e.Message, e);
            }
        }

        protected override void ExtractPrimaryKeyData(ITransaction tx, MetaTable table)
        {
            try
            {
                var cmd = tx.CreateCommand();
                cmd.CommandText = $" SELECT Constraint_Name,Column_Name FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE" +
                                  $" WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1" +
                                  $" AND table_name = '[{table.Name}]';";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (table.PrimaryKey == null)
                        {
                            var primaryKey = new MetaPrimaryKey();
                            table.PrimaryKey = primaryKey;
                            primaryKey.ColumnNames = new List<string>();
                            primaryKey.Name = reader["Constraint_Name"].ToString();
                        }
                        table.PrimaryKey.ColumnNames.Add(reader["Column_Name"].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    string.Format("Exception occured while trying to read primary key in table {0}", table.Name), e);
                throw new DBPatchingException(e.Message, e);
            }
        }

        protected override void ExtractForeignKeyData(ITransaction tx, MetaTable table)
        {
            try
            {
                var cmd = tx.CreateCommand();
                cmd.CommandText = $"SELECT f.name AS ForeignKey," +
                                  $" OBJECT_NAME(f.parent_object_id) AS TableName," +
                                  $" COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName," +
                                  $" OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName," +
                                  $" COL_NAME(fc.referenced_object_id," +
                                  $" fc.referenced_column_id) AS ReferenceColumnName," +
                                  $" f.update_referential_action as updateAction," +
                                  $" f.delete_referential_action as deleteAction " +
                                  $"FROM sys.foreign_keys AS f INNER JOIN sys.foreign_key_columns AS fc " +
                                  $" ON f.OBJECT_ID = fc.constraint_object_id " +
                                  $"WHERE OBJECT_NAME(f.parent_object_id) = '{table.Name}'; ";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var foreignKeyName = reader["ForeignKey"].ToString();
                        var foreignKey = table.ForeignKeys.SingleOrDefault(fk => fk.Name == foreignKeyName);

                        if (foreignKey == null)
                        {
                            foreignKey = new MetaForeignKey();
                            foreignKey.Name = foreignKeyName;
                            foreignKey.ToTable = reader["ReferenceTableName"].ToString();
                            foreignKey.UpdateRule = (ReferentialRuleType) 
                                int.Parse(reader["updateAction"].ToString());
                            foreignKey.DeleteRule = (ReferentialRuleType) 
                                int.Parse(reader["deleteAction"].ToString());
                        }

                        foreignKey.ColumnMappings.Add(new MetaForeignKeyColumnMapping(
                            reader["columnName"].ToString(),
                            reader["referenceColumnName"].ToString())
                        );
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    string.Format("Exception occured while trying to read primary key in table {0}", table.Name), e);
                throw new DBPatchingException(e.Message, e);
            }
        }

        protected override string CreateCreateTableQuery(MetaComparisonTableGroup tableGroup)
        {
            var metaTable = (MetaTable)tableGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE [");
            sb.Append(metaTable.Name);
            sb.Append("] ( ");
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
                    sb.Append(metaColumn.Size > 0 ? "" + metaColumn.Size : "max");
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

            sb.Append(")");
            return sb.ToString();
        }

        protected override string CreateDropTableQuery(MetaComparisonTableGroup tableGroup)
        {
            var metaTable = (MetaTable)tableGroup.ExistingItem;

            var sb = new StringBuilder();
            sb.Append("DROP TABLE [");
            sb.Append(metaTable.Name);
            sb.Append("]");
            return sb.ToString();
        }

        protected override string CreateAlterTableQuery(MetaComparisonTableGroup tableGroup)
        {
            return null;
        }

        protected override string CreateCreateColumnQuery(MetaComparisonTableGroup tableGroup,
                                                          MetaComparisonColumnGroup columnGroup)
        {
            var metaTable = (MetaTable)tableGroup.RequiredItem;
            var metaColumn = (MetaColumn)columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(metaTable.Name);
            sb.Append("] ADD ");
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
                string defaultValue = GetDefaultValueForType(metaColumn.ColumnType);
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

        protected override string CreateDropColumnQuery(MetaComparisonTableGroup tableGroup,
                                                        MetaComparisonColumnGroup columnGroup)
        {
            var metaTable = (MetaTable)tableGroup.RequiredItem;
            var metaColumn = (MetaColumn)columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(metaTable.Name);
            sb.Append("] DROP COLUMN ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");

            return sb.ToString();
        }

        protected override string CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup,
                                                         MetaComparisonColumnGroup columnGroup)
        {
            var metaTable = (MetaTable)tableGroup.RequiredItem;
            var metaColumn = (MetaColumn)columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(metaTable.Name);
            sb.Append("] ALTER ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");
            sb.Append(" SET DATA TYPE ");
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
                string defaultValue = GetDefaultValueForType(metaColumn.ColumnType);
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

        protected override string CreateCreatePrimaryKeyQuery(MetaComparisonTableGroup tableGroup,
                                                              MetaComparisonPrimaryKeyGroup primaryKeyGroup)
        {
            var requiredTable = (MetaTable)tableGroup.RequiredItem;
            var primaryKey = (MetaPrimaryKey)primaryKeyGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(requiredTable.Name);
            sb.Append("] ADD CONSTRAINT ");
            sb.Append(primaryKey.Name);
            sb.Append(" PRIMARY KEY ( ");

            bool first = true;
            foreach (String columnName in primaryKey.ColumnNames)
            {
                if (!first)
                {
                    sb.Append(" ,\n ");
                }
                else
                {
                    first = false;
                }
                sb.Append(columnName);
                sb.Append(" ");
            }

            sb.Append(")");
            return sb.ToString();
        }

        protected override string CreateDropPrimaryKeyQuery(MetaComparisonTableGroup tableGroup,
                                                            MetaComparisonPrimaryKeyGroup primaryKeyGroup)
        {
            var requiredTable = (MetaTable)tableGroup.ExistingItem;
            var primaryKey = (MetaPrimaryKey)primaryKeyGroup.ExistingItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(requiredTable.Name);
            sb.Append("] DROP CONSTRAINT ");
            sb.Append(primaryKey.Name);
            return sb.ToString();
        }

        protected override string CreateCreateForeginKeyQuery(MetaComparisonTableGroup tableGroup,
                                                              MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            var requiredTable = (MetaTable)tableGroup.RequiredItem;
            var metaForeignKey = (MetaForeignKey)foreignKeyGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(requiredTable.Name);
            sb.Append("] ADD CONSTRAINT ");
            sb.Append(metaForeignKey.Name);
            sb.Append(" FOREIGN KEY ");

            sb.Append(" ( ");
            IEnumerator<MetaForeignKeyColumnMapping> enumerator = metaForeignKey.ColumnMappings.GetEnumerator();
            enumerator.Reset();

            int i = 0;
            while (enumerator.MoveNext())
            {
                MetaForeignKeyColumnMapping mapping = enumerator.Current;
                if (i > 0)
                {
                    sb.Append(" , ");
                }
                sb.Append(mapping.FromColumn);
                i++;
            }
            sb.Append(" ) ");

            sb.Append(" REFERENCES ");
            sb.Append(metaForeignKey.ToTable);

            sb.Append(" ( ");
            enumerator = metaForeignKey.ColumnMappings.GetEnumerator();
            enumerator.Reset();
            i = 0;
            while (enumerator.MoveNext())
            {
                MetaForeignKeyColumnMapping mapping = enumerator.Current;
                if (i > 0)
                {
                    sb.Append(" , ");
                }
                sb.Append(mapping.ToColumn);
                i++;
            }
            sb.Append(" ) ");
            sb.Append(" ON DELETE ").Append(metaForeignKey.DeleteRule);
            if (metaForeignKey.UpdateRule != ReferentialRuleType.Restrict)
            {
                sb.Append(" ON UPDATE ").Append(metaForeignKey.UpdateRule);
            }

            return sb.ToString();
        }

        protected override string CreateDropForeignKeyQuery(MetaComparisonTableGroup tableGroup,
                                                            MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            var requiredTable = (MetaTable)tableGroup.RequiredItem;
            var metaForeignKey = (MetaForeignKey)foreignKeyGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE [");
            sb.Append(requiredTable.Name);
            sb.Append("] DROP CONSTRAINT ");
            sb.Append(metaForeignKey.ToTable);
            return sb.ToString();
        }
    }
}