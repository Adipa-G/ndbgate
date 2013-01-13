using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.compare;
using dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.datastructures;
using log4net;

namespace dbgate.ermanagement.impl.dbabstractionlayer.metamanipulate.dbmm.defaultmm
{
    public class DefaultMetaManipulate : AbstractMetaManipulate
    {
        public DefaultMetaManipulate(IDbLayer dbLayer,IErLayerConfig config) : base(dbLayer,config)
        {
        }

        protected override ICollection<MetaTable> ExtractTableData(IDbConnection con)
        {
            List<MetaTable> metaItems = new List<MetaTable>();
            try
            {
                DbConnection dbConnection = (DbConnection)con;
                DataTable dbTableTable = dbConnection.GetSchema("Tables");
                if (dbTableTable == null)
                {
                    LogManager.GetLogger(Config.LoggerName).Fatal("Unable to read the list of tables");
                    return null;
                }
                foreach (DataRow tableRow in dbTableTable.Rows)
                {
                    MetaTable table = new MetaTable();
                    metaItems.Add(table);
                    table.Name = tableRow["TABLE_NAME"].ToString();
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal("Exception occured while trying to read table information", e);
                throw new DBPatchingException(e.Message, e);
            }
            return metaItems;
        }

        protected override void ExtractColumnData(IDbConnection con, MetaTable table)
        {
            try
            {
                DbConnection dbConnection = (DbConnection) con;
                DataTable columnTable = dbConnection.GetSchema("Columns", new string[]{null,null,table.Name,null});
                if (columnTable == null)
                {
                    LogManager.GetLogger(Config.LoggerName).Fatal(string.Format("Unable to read the list of columns in table {0}", table.Name));                        
                    return;
                }
                foreach (DataRow columnRow in columnTable.Rows)
                {
                    MetaColumn column = new MetaColumn();
                    table.Columns.Add(column);
                    column.Name = columnRow["COLUMN_NAME"].ToString();
                    column.Size = (int)columnRow["CHARACTER_MAXIMUM_LENGTH"];
                    column.ColumnType = MapColumnTypeNameToType(columnRow["DATA_TYPE"].ToString());
                    column.Null = (bool)columnRow["IS_NULLABLE"];
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal(string.Format("Exception occured while trying to read column information in table {0}", table.Name), e);
                throw new DBPatchingException(e.Message, e);
            } 
        }

        protected override void ExtractPrimaryKeyData(IDbConnection con, MetaTable table)
        {
            Dictionary<int,string> keyColMap = new Dictionary<int, string>();
            try
            {
                DbConnection dbConnection = (DbConnection) con;
                DataTable pkTable = dbConnection.GetSchema("Primary_Keys", new string[]{null,null,table.Name,null});
                if (pkTable == null)
                {
                    LogManager.GetLogger(Config.LoggerName).Fatal(string.Format("Unable to read the primary key in table {0}", table.Name));                        
                    return;
                }
                foreach (DataRow pkRow in pkTable.Rows)
                {
                    if (table.PrimaryKey == null)
                    {
                        MetaPrimaryKey primaryKey = new MetaPrimaryKey();
                        table.PrimaryKey = primaryKey;
                        primaryKey.Name = pkRow["PK_NAME"].ToString();
                    }
                    keyColMap.Add((int)pkRow["ORDINAL"],pkRow["COLUMN_NAME"].ToString());
                }
                if (table.PrimaryKey != null)
                {
                    List<int> list = new List<int>(keyColMap.Keys);
                    list.Sort();
                    foreach (int ordinal in list)
                    {
                        table.PrimaryKey.ColumnNames.Add(keyColMap[ordinal]);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal(string.Format("Exception occured while trying to read primary key in table {0}", table.Name), e);
                throw new DBPatchingException(e.Message, e);
            } 
        }

        protected override void ExtractForeignKeyData(IDbConnection con, MetaTable table)
        {
            Dictionary<string, MetaForeignKey> foreignKeyMap = new Dictionary<string, MetaForeignKey>();

            Dictionary<string,Dictionary<int,string>> fromTableColMap = new Dictionary<string, Dictionary<int, string>>();
            Dictionary<string,Dictionary<int,string>> toTableColMap = new Dictionary<string, Dictionary<int, string>>();

            try
            {
                DbConnection dbConnection = (DbConnection) con;
                DataTable fkTable = dbConnection.GetSchema("Foreign_Keys", new string[]{null,null,table.Name,null});
                if (fkTable == null)
                {
                    LogManager.GetLogger(Config.LoggerName).Fatal(string.Format("Unable to read the list of foregin keys in table {0}", table.Name));                        
                    return;
                }
                foreach (DataRow fkRow in fkTable.Rows)
                {
                    String fkName = fkRow["FK_NAME"].ToString();
                    if (!foreignKeyMap.ContainsKey(fkName))
                    {
                        MetaForeignKey foreignKey = new MetaForeignKey();
                        table.ForeignKeys.Add(foreignKey);
                        foreignKeyMap.Add(fkName,foreignKey);
                        foreignKey.Name = fkName;
                        foreignKey.ToTable = fkRow["FK_TABLE_NAME"].ToString();
                        foreignKey.UpdateRule = (ReferentialRuleType)fkRow["UPDATE_RULE"];
                        foreignKey.DeleteRule = (ReferentialRuleType) fkRow["DELETE_RULE"];

                        fromTableColMap.Add(fkName,new Dictionary<int, string>());
                        toTableColMap.Add(fkName,new Dictionary<int, string>());
                    }
                    
                    fromTableColMap[fkName].Add(fromTableColMap[fkName].Count,fkRow["PK_COLUMN_NAME"].ToString());
                    toTableColMap[fkName].Add(toTableColMap[fkName].Count,fkRow["FK_COLUMN_NAME"].ToString());
                }

                foreach (string key in foreignKeyMap.Keys)
                {
                    MetaForeignKey foreignKey = foreignKeyMap[key];

                    List<int> fromList = new List<int>(fromTableColMap[key].Keys);
                    fromList.Sort();

                    List<int> toList = new List<int>(toTableColMap[key].Keys);
                    toList.Sort();

                    IEnumerator<int> toListEnumerator = toList.GetEnumerator();
                    toListEnumerator.Reset();

                    foreach (int ordinal in fromList)
                    {
                        String fromCol = fromTableColMap[key][ordinal];
                        String toCol = null;
                        if (toListEnumerator.MoveNext())
                        {
                            toCol = toTableColMap[key][toListEnumerator.Current];
                        }
                        foreignKey.ColumnMappings.Add(new MetaForeignKeyColumnMapping(fromCol,toCol));
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal(string.Format("Exception occured while trying to read foreign key information in table {0}", table.Name), e);
                throw new DBPatchingException(e.Message, e);
            } 
        }
        
        protected override String CreateCreateTableQuery(MetaComparisonTableGroup tableGroup)
        {
            MetaTable metaTable = (MetaTable)tableGroup.RequiredItem;

            StringBuilder sb = new StringBuilder();
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
                if (metaColumn.ColumnType == DbColumnType.Char
                        || metaColumn.ColumnType == DbColumnType.Varchar)
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

        protected override String CreateDropTableQuery(MetaComparisonTableGroup tableGroup)
        {
            MetaTable metaTable = (MetaTable)tableGroup.ExistingItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("DROP TABLE ");
            sb.Append(metaTable.Name);
            return sb.ToString();
        }

        protected override String CreateAlterTableQuery(MetaComparisonTableGroup tableGroup)
        {
            return null;
        }

        protected override String CreateCreateColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup)
        {
            MetaTable metaTable = (MetaTable)tableGroup.RequiredItem;
            MetaColumn metaColumn = (MetaColumn)columnGroup.RequiredItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" ADD ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");
            if (metaColumn.ColumnType == DbColumnType.Char
                    || metaColumn.ColumnType == DbColumnType.Varchar)
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

        protected override String CreateDropColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup)
        {
            MetaTable metaTable = (MetaTable)tableGroup.RequiredItem;
            MetaColumn metaColumn = (MetaColumn)columnGroup.RequiredItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" DROP COLUMN ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");

            return sb.ToString();
        }

        protected override String CreateAlterColumnQuery(MetaComparisonTableGroup tableGroup, MetaComparisonColumnGroup columnGroup)
        {
            MetaTable metaTable = (MetaTable)tableGroup.RequiredItem;
            MetaColumn metaColumn = (MetaColumn)columnGroup.RequiredItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaTable.Name);
            sb.Append(" ALTER ");
            sb.Append(metaColumn.Name);
            sb.Append(" ");
            sb.Append(" SET DATA TYPE ");
            if (metaColumn.ColumnType == DbColumnType.Char
                    || metaColumn.ColumnType == DbColumnType.Varchar)
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

        protected override String CreateCreatePrimaryKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonPrimaryKeyGroup primaryKeyGroup)
        {
            MetaTable requiredTable = (MetaTable)tableGroup.RequiredItem;
            MetaPrimaryKey primaryKey = (MetaPrimaryKey)primaryKeyGroup.RequiredItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" ADD CONSTRAINT ");
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

        protected override String CreateDropPrimaryKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonPrimaryKeyGroup primaryKeyGroup)
        {
            MetaTable requiredTable = (MetaTable)tableGroup.ExistingItem;
            MetaPrimaryKey primaryKey = (MetaPrimaryKey)primaryKeyGroup.ExistingItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" DROP CONSTRAINT ");
            sb.Append(primaryKey.Name);
            return sb.ToString();
        }

        protected override String CreateCreateForeginKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            MetaTable requiredTable = (MetaTable)tableGroup.RequiredItem;
            MetaForeignKey metaForeignKey = (MetaForeignKey)foreignKeyGroup.RequiredItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(metaForeignKey.ToTable);
            sb.Append(" ADD CONSTRAINT ");
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
                sb.Append(mapping.ToColumn);
                i++;
            }
            sb.Append(" ) ");

            sb.Append(" REFERENCES ");
            sb.Append(requiredTable.Name);

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
                sb.Append(mapping.FromColumn);
                i++;
            }
            sb.Append(" ) ");
            sb.Append(" ON DELETE ").Append(metaForeignKey.DeleteRule);
            sb.Append(" ON UPDATE ").Append(metaForeignKey.UpdateRule);

            return sb.ToString();
        }

        protected override String CreateDropForeginKeyQuery(MetaComparisonTableGroup tableGroup, MetaComparisonForeignKeyGroup foreignKeyGroup)
        {
            MetaTable requiredTable = (MetaTable)tableGroup.ExistingItem;
            MetaForeignKey metaForeignKey = (MetaForeignKey)foreignKeyGroup.ExistingItem;

            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(requiredTable.Name);
            sb.Append(" DROP CONSTRAINT ");
            sb.Append(metaForeignKey.Name);
            return sb.ToString();
        }
    }
}
