﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Compare;
using DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DataStructures;
using DbGate.Exceptions;
using log4net;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.DbMm.DefaultMm
{
    public class DefaultMetaManipulate : AbstractMetaManipulate
    {
        public DefaultMetaManipulate(IDbLayer dbLayer, IDbGateConfig config) : base(dbLayer, config)
        {
        }

        protected override ICollection<MetaTable> ExtractTableData(ITransaction tx)
        {
            var metaItems = new List<MetaTable>();
            try
            {
                var dbConnection = (DbConnection) tx.Connection;
                var dbTableTable = dbConnection.GetSchema("Tables");
                if (dbTableTable == null)
                {
                    Logger.GetLogger(Config.LoggerName).Fatal("Unable to read the list of tables");
                    return null;
                }
                foreach (DataRow tableRow in dbTableTable.Rows)
                {
                    var table = new MetaTable();
                    metaItems.Add(table);
                    table.Name = tableRow["TABLE_NAME"].ToString();
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    "Exception occured while trying to read table information", e);
                throw new DbPatchingException(e.Message, e);
            }
            return metaItems;
        }

        protected override void ExtractColumnData(ITransaction tx, MetaTable table)
        {
            try
            {
                var dbConnection = (DbConnection) tx.Connection;
                var columnTable = dbConnection.GetSchema("Columns", new[] {null, null, table.Name, null});
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
                    column.Name = columnRow["COLUMN_NAME"].ToString();
                    column.Size = (int) columnRow["CHARACTER_MAXIMUM_LENGTH"];
                    column.ColumnType = MapColumnTypeNameToType(columnRow["DATA_TYPE"].ToString());
                    column.Null = (bool) columnRow["IS_NULLABLE"];
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    string.Format("Exception occured while trying to read column information in table {0}", table.Name),
                    e);
                throw new DbPatchingException(e.Message, e);
            }
        }

        protected override void ExtractPrimaryKeyData(ITransaction tx, MetaTable table)
        {
            var keyColMap = new Dictionary<int, string>();
            try
            {
                var dbConnection = (DbConnection) tx.Connection;
                var pkTable = dbConnection.GetSchema("Primary_Keys", new[] {null, null, table.Name, null});
                if (pkTable == null)
                {
                    Logger.GetLogger(Config.LoggerName).Fatal(
                        string.Format("Unable to read the primary key in table {0}", table.Name));
                    return;
                }
                foreach (DataRow pkRow in pkTable.Rows)
                {
                    if (table.PrimaryKey == null)
                    {
                        var primaryKey = new MetaPrimaryKey();
                        table.PrimaryKey = primaryKey;
                        primaryKey.Name = pkRow["PK_NAME"].ToString();
                    }
                    keyColMap.Add((int) pkRow["ORDINAL"], pkRow["COLUMN_NAME"].ToString());
                }
                if (table.PrimaryKey != null)
                {
                    var list = new List<int>(keyColMap.Keys);
                    list.Sort();
                    foreach (var ordinal in list)
                    {
                        table.PrimaryKey.ColumnNames.Add(keyColMap[ordinal]);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    string.Format("Exception occured while trying to read primary key in table {0}", table.Name), e);
                throw new DbPatchingException(e.Message, e);
            }
        }

        protected override void ExtractForeignKeyData(ITransaction tx, MetaTable table)
        {
            var foreignKeyMap = new Dictionary<string, MetaForeignKey>();

            var fromTableColMap = new Dictionary<string, Dictionary<int, string>>();
            var toTableColMap = new Dictionary<string, Dictionary<int, string>>();

            try
            {
                var dbConnection = (DbConnection) tx.Connection;
                var fkTable = dbConnection.GetSchema("Foreign_Keys", new[] {null, null, table.Name, null});
                if (fkTable == null)
                {
                    Logger.GetLogger(Config.LoggerName).Fatal(
                        string.Format("Unable to read the list of foregin keys in table {0}", table.Name));
                    return;
                }
                foreach (DataRow fkRow in fkTable.Rows)
                {
                    var fkName = fkRow["FK_NAME"].ToString();
                    if (!foreignKeyMap.ContainsKey(fkName))
                    {
                        var foreignKey = new MetaForeignKey();
                        table.ForeignKeys.Add(foreignKey);
                        foreignKeyMap.Add(fkName, foreignKey);
                        foreignKey.Name = fkName;
                        foreignKey.ToTable = fkRow["FK_TABLE_NAME"].ToString();
                        foreignKey.UpdateRule = (ReferentialRuleType) fkRow["UPDATE_RULE"];
                        foreignKey.DeleteRule = (ReferentialRuleType) fkRow["DELETE_RULE"];

                        fromTableColMap.Add(fkName, new Dictionary<int, string>());
                        toTableColMap.Add(fkName, new Dictionary<int, string>());
                    }

                    fromTableColMap[fkName].Add(fromTableColMap[fkName].Count, fkRow["PK_COLUMN_NAME"].ToString());
                    toTableColMap[fkName].Add(toTableColMap[fkName].Count, fkRow["FK_COLUMN_NAME"].ToString());
                }

                foreach (var key in foreignKeyMap.Keys)
                {
                    var foreignKey = foreignKeyMap[key];

                    var fromList = new List<int>(fromTableColMap[key].Keys);
                    fromList.Sort();

                    var toList = new List<int>(toTableColMap[key].Keys);
                    toList.Sort();

                    IEnumerator<int> toListEnumerator = toList.GetEnumerator();
                    toListEnumerator.Reset();

                    foreach (var ordinal in fromList)
                    {
                        var fromCol = fromTableColMap[key][ordinal];
                        string toCol = null;
                        if (toListEnumerator.MoveNext())
                        {
                            toCol = toTableColMap[key][toListEnumerator.Current];
                        }
                        foreignKey.ColumnMappings.Add(new MetaForeignKeyColumnMapping(fromCol, toCol));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(Config.LoggerName).Fatal(
                    string.Format("Exception occured while trying to read foreign key information in table {0}",
                                  table.Name), e);
                throw new DbPatchingException(e.Message, e);
            }
        }

        protected override string CreateCreateTableQuery(MetaComparisonTableGroup tableGroup)
        {
            var metaTable = (MetaTable) tableGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" ( ");
            var first = true;

            foreach (var metaColumn in metaTable.Columns)
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

            sb.Append(")");
            return sb.ToString();
        }

        protected override string CreateDropTableQuery(MetaComparisonTableGroup tableGroup)
        {
            var metaTable = (MetaTable) tableGroup.ExistingItem;

            var sb = new StringBuilder();
            sb.Append("DROP TABLE ");
            sb.Append(metaTable.Name);
            return sb.ToString();
        }

        protected override string CreateAlterTableQuery(MetaComparisonTableGroup tableGroup)
        {
            return null;
        }

        protected override string CreateCreateColumnQuery(MetaComparisonTableGroup tableGroup,
                                                          MetaComparisonColumnGroup columnGroup)
        {
            var metaTable = (MetaTable) tableGroup.RequiredItem;
            var metaColumn = (MetaColumn) columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" ADD ");
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
                var defaultValue = GetDefaultValueForType(metaColumn.ColumnType);
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
            var metaTable = (MetaTable) tableGroup.RequiredItem;
            var metaColumn = (MetaColumn) columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" DROP COLUMN ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");

            return sb.ToString();
        }

        protected override string CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup,
                                                         MetaComparisonColumnGroup columnGroup)
        {
            var metaTable = (MetaTable) tableGroup.RequiredItem;
            var metaColumn = (MetaColumn) columnGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" ALTER ");
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
                var defaultValue = GetDefaultValueForType(metaColumn.ColumnType);
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
            var requiredTable = (MetaTable) tableGroup.RequiredItem;
            var primaryKey = (MetaPrimaryKey) primaryKeyGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" ADD CONSTRAINT ");
            sb.Append(primaryKey.Name);
            sb.Append(" PRIMARY KEY ( ");

            var first = true;
            foreach (var columnName in primaryKey.ColumnNames)
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
            var requiredTable = (MetaTable) tableGroup.ExistingItem;
            var primaryKey = (MetaPrimaryKey) primaryKeyGroup.ExistingItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" DROP CONSTRAINT ");
            sb.Append(primaryKey.Name);
            return sb.ToString();
        }

        protected override string CreateCreateForeginKeyQuery(MetaComparisonTableGroup tableGroup,
                                                              MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            var requiredTable = (MetaTable) tableGroup.RequiredItem;
            var metaForeignKey = (MetaForeignKey) foreignKeyGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" ADD CONSTRAINT ");
            sb.Append(metaForeignKey.Name);
            sb.Append(" FOREIGN KEY ");

            sb.Append(" ( ");
            var enumerator = metaForeignKey.ColumnMappings.GetEnumerator();
            enumerator.Reset();

            var i = 0;
            while (enumerator.MoveNext())
            {
                var mapping = enumerator.Current;
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
                var mapping = enumerator.Current;
                if (i > 0)
                {
                    sb.Append(" , ");
                }
                sb.Append(mapping.ToColumn);
                i++;
            }
            sb.Append(" ) ");
            sb.Append(" ON DELETE ").Append(metaForeignKey.DeleteRule);
            sb.Append(" ON UPDATE ").Append(metaForeignKey.UpdateRule);

            return sb.ToString();
        }

        protected override string CreateDropForeignKeyQuery(MetaComparisonTableGroup tableGroup,
                                                            MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            var requiredTable = (MetaTable) tableGroup.RequiredItem;
            var metaForeignKey = (MetaForeignKey) foreignKeyGroup.RequiredItem;

            var sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" DROP CONSTRAINT ");
            sb.Append(metaForeignKey.ToTable);
            return sb.ToString();
        }
    }
}