using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.utils;
using log4net;

namespace dbgate.ermanagement.impl
{
    public class ErDataRetrievalManager : ErDataCommonManager
    {
        public ErDataRetrievalManager(IDbLayer dbLayer, IErLayerConfig config)
            : base(dbLayer, config)
        {
        }

        public void Load(IServerRoDbClass roEntity, IDataReader reader, IDbConnection con)
        {
            if (roEntity is IServerDbClass)
            {
                IServerDbClass entity = (IServerDbClass)roEntity;
                entity.Status = DbClassStatus.Unmodified;
            }
            try
            {
                ErSessionUtils.InitSession(roEntity);
                ErDataManagerUtils.RegisterTypes(roEntity);
                LoadFromDb(roEntity, reader, con);
                ErSessionUtils.DestroySession(roEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(Config.LoggerName).Fatal(e.Message, e);
                throw new RetrievalException(e.Message, e);
            }
        }

        private void LoadFromDb(IServerRoDbClass roEntity, IDataReader reader, IDbConnection con)
        {
            Type[] typeList = ReflectionUtils.GetSuperTypesWithInterfacesImplemented(roEntity.GetType(), new[] { typeof(IServerRoDbClass) });
            int typeListLength = typeList.Length;
            for (int i = 0; i < typeListLength; i++)
            {
                Type type = typeList[i];
                String tableName = CacheManager.TableCache.GetTableName(type);
                if (i == 0 || tableName == null) //if i==0 that means it's base class and can use existing result set
                {
                    LoadForType(roEntity, type, reader, con);
                }
                else
                {
                    IDbCommand superCmd = null;
                    IDataReader superReader = null;
                    try
                    {
                        ITypeFieldValueList keyValueList = ErDataManagerUtils.ExtractTypeKeyValues(roEntity, type);
                        superCmd = CreateRetrievalPreparedStatement(keyValueList, con);
                        superReader = superCmd.ExecuteReader();
                        if (superReader.Read())
                        {
                            LoadForType(roEntity, type, superReader, con);
                        }
                        else
                        {
                            String message = String.Format("Super class {0} does not contains a matching record for the base class {1}", type.FullName, typeList[0].FullName);
                            throw new NoMatchingRecordFoundForSuperClassException(message);
                        }
                    }
                    finally
                    {
                        DbMgmtUtility.Close(superReader);
                        DbMgmtUtility.Close(superCmd);
                    }
                }
            }
        }

        private void LoadForType(IServerRoDbClass entity, Type type, IDataReader reader, IDbConnection con)
        {
            IEntityContext entityContext = entity.Context;
            ITypeFieldValueList valueTypeList = ReadValues(type, reader);
            SetValues(entity, valueTypeList);
            ErSessionUtils.AddToSession(entity, ErDataManagerUtils.ExtractEntityKeyValues(entity));

            if (entityContext != null)
            {
                foreach (EntityFieldValue fieldValue in valueTypeList.FieldValues)
                {
                    entityContext.ChangeTracker.Fields.Add(fieldValue);
                }
            }

            ICollection<IDbRelation> dbRelations = CacheManager.FieldCache.GetDbRelations(type);
            foreach (IDbRelation relation in dbRelations)
            {
                LoadChildrenFromRelation(entity, type, con, relation);
            }
        }

        private void LoadChildrenFromRelation(IServerRoDbClass parentRoEntity, Type type, IDbConnection con
            , IDbRelation relation)
        {
            IEntityContext entityContext = parentRoEntity.Context;

            PropertyInfo getter = CacheManager.MethodCache.GetProperty(parentRoEntity, relation.AttributeName);
            Object value = getter.GetValue(parentRoEntity, null);

            ICollection<IServerRoDbClass> children = ReadRelationChildrenFromDb(parentRoEntity, type, con, relation);
            if (entityContext != null
                    && !relation.ReverseRelationship)
            {
                foreach (IServerRoDbClass childEntity in children)
                {
                    ITypeFieldValueList valueTypeList = ErDataManagerUtils.ExtractRelationKeyValues(childEntity, relation);
                    if (valueTypeList != null)
                    {
                        entityContext.ChangeTracker.ChildEntityKeys.Add(valueTypeList);
                    }
                }
            }

            if (value == null
                    && ReflectionUtils.IsImplementInterface(getter.PropertyType, typeof(ICollection<>)))
            {
                PropertyInfo setter = CacheManager.MethodCache.GetProperty(parentRoEntity, relation.AttributeName);
                value = Activator.CreateInstance(setter.PropertyType);

                IList genCollection = (IList)value;
                foreach (IServerRoDbClass serverRoDbClass in children)
                {
                    genCollection.Add(serverRoDbClass);
                }
                setter.SetValue(parentRoEntity, genCollection, null);
            }
            else if (value != null
                    && ReflectionUtils.IsImplementInterface(getter.PropertyType, typeof(ICollection<>)))
            {
                IList genCollection = (IList)value;
                foreach (IServerRoDbClass serverRoDbClass in children)
                {
                    genCollection.Add(serverRoDbClass);
                }
            }
            else
            {
                IEnumerator<IServerRoDbClass> childEnumarator = children.GetEnumerator();
                if (childEnumarator.MoveNext())
                {
                    IServerRoDbClass singleRoDbClass = childEnumarator.Current;
                    if (getter.PropertyType.IsAssignableFrom(singleRoDbClass.GetType()))
                    {
                        PropertyInfo setter = CacheManager.MethodCache.GetProperty(parentRoEntity, relation.AttributeName);
                        setter.SetValue(parentRoEntity, singleRoDbClass, null);
                    }
                    else
                    {
                        string message = singleRoDbClass.GetType().FullName + " is not matching the getter " + getter.Name;
                        LogManager.GetLogger(Config.LoggerName).Fatal(message);
                        throw new NoSetterFoundToSetChildObjectListException(message);
                    }
                }
            }
        }

        private ICollection<IServerRoDbClass> ReadRelationChildrenFromDb(IServerRoDbClass entity, Type type
                , IDbConnection con, IDbRelation relation)
        {
            Type childType = relation.RelatedObjectType;
            IServerRoDbClass childTypeInstance = (IServerRoDbClass)Activator.CreateInstance(childType);
            ErDataManagerUtils.RegisterTypes(childTypeInstance);

            StringBuilder logSb = new StringBuilder();
            String query = CacheManager.QueryCache.GetRelationObjectLoad(entity.GetType(), relation);

            IList<string> fields = new List<string>();
            foreach (DbRelationColumnMapping mapping in relation.TableColumnMappings)
            {
                fields.Add(mapping.FromField);
            }

            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = query;
            bool showQuery = Config.ShowQueries;
            if (showQuery)
            {
                logSb.Append(query);
            }
            ICollection<IDbColumn> dbColumns = CacheManager.FieldCache.GetDbColumns(type);
            for (int i = 0; i < fields.Count; i++)
            {
                String field = fields[i];
                IDbColumn matchColumn = ErDataManagerUtils.FindColumnByAttribute(dbColumns, field);

                if (matchColumn != null)
                {
                    PropertyInfo getter = CacheManager.MethodCache.GetProperty(entity, matchColumn.AttributeName);
                    Object fieldValue = getter.GetValue(entity, null);

                    if (showQuery)
                    {
                        logSb.Append(" ,").Append(matchColumn.ColumnName).Append("=").Append(fieldValue);
                    }
                    DbLayer.GetDataManipulate().SetToPreparedStatement(cmd, fieldValue, i + 1, matchColumn);
                }
                else
                {
                    String message = String.Format("The field {0} does not have a matching field in the object {1}", field, entity.GetType().FullName);
                    throw new NoMatchingColumnFoundException(message);
                }
            }
            if (showQuery)
            {
                LogManager.GetLogger(Config.LoggerName).Info(logSb.ToString());
            }
            return ReadFromPreparedStatement(entity, con, cmd, childType);
        }

        private ICollection<IServerRoDbClass> ReadFromPreparedStatement(IServerRoDbClass entity, IDbConnection con, IDbCommand cmd
            , Type childType)
        {
            ICollection<IDbColumn> childKeys = null;
            ICollection<IServerRoDbClass> data = new List<IServerRoDbClass>();
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (childKeys == null)
                {
                    childKeys = CacheManager.FieldCache.GetKeys(childType);
                }
                ITypeFieldValueList childTypeKeyList = new EntityTypeFieldValueList(childType);
                foreach (IDbColumn childKey in childKeys)
                {
                    Object value = DbLayer.GetDataManipulate().ReadFromResultSet(reader, childKey);
                    childTypeKeyList.FieldValues.Add(new EntityFieldValue(value, childKey));
                }
                if (ErSessionUtils.ExistsInSession(entity, childTypeKeyList))
                {
                    data.Add(ErSessionUtils.GetFromSession(entity, childTypeKeyList));
                    continue;
                }

                IServerRoDbClass rodbClass = (IServerRoDbClass)Activator.CreateInstance(childType);
                ErSessionUtils.TransferSession(entity, rodbClass);
                rodbClass.Retrieve(reader, con);
                data.Add(rodbClass);

                IEntityFieldValueList childEntityKeyList = ErDataManagerUtils.ExtractEntityKeyValues(rodbClass);
                ErSessionUtils.AddToSession(entity, childEntityKeyList);
            }
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);
            return data;
        }
    }
}
