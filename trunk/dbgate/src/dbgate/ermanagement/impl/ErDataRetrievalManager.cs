using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl.dbabstractionlayer;
using dbgate.ermanagement.impl.utils;
using dbgate.ermanagement.lazy;
using log4net;

namespace dbgate.ermanagement.impl
{
    public class ErDataRetrievalManager : ErDataCommonManager
    {
        private ProxyGenerator _proxyGenerator;

        public ErDataRetrievalManager(IDbLayer dbLayer,IErLayerStatistics statistics, IErLayerConfig config)
            : base(dbLayer,statistics, config)
        {
            _proxyGenerator = new ProxyGenerator();
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
                LoadChildrenFromRelation(entity, type, con, relation,false);
            }
        }

        public void LoadChildrenFromRelation(IServerRoDbClass parentRoEntity, Type type, IDbConnection con
            , IDbRelation relation,bool lazy)
        {
            IEntityContext entityContext = parentRoEntity.Context;

            PropertyInfo property = CacheManager.MethodCache.GetProperty(parentRoEntity, relation.AttributeName);
            Object value = property.GetValue(parentRoEntity, null);
            
            if (!lazy && relation.Lazy)
            {
                CreateProxy(parentRoEntity, type, con, relation, value, property);
                return;
            }

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

            if ((value == null || ProxyUtil.IsProxyType(value.GetType()))
                    && ReflectionUtils.IsImplementInterface(property.PropertyType, typeof(ICollection<>)))
            {
                Type propertyType = property.PropertyType;
                if (propertyType.IsInterface)
                {
                    Type generic = propertyType.GetGenericArguments()[0];
                    propertyType = typeof(List<>).MakeGenericType(new Type[] { generic });
                }
                value = Activator.CreateInstance(propertyType);

                IList genCollection = (IList)value;
                foreach (IServerRoDbClass serverRoDbClass in children)
                {
                    genCollection.Add(serverRoDbClass);
                }
                property.SetValue(parentRoEntity, genCollection, null);
            }
            else if (value != null
                    && ReflectionUtils.IsImplementInterface(property.PropertyType, typeof(ICollection<>)))
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
                    if (property.PropertyType.IsAssignableFrom(singleRoDbClass.GetType()))
                    {
                        property.SetValue(parentRoEntity, singleRoDbClass, null);
                    }
                    else
                    {
                        string message = singleRoDbClass.GetType().FullName + " is not matching the getter " + property.Name;
                        LogManager.GetLogger(Config.LoggerName).Fatal(message);
                        throw new NoSetterFoundToSetChildObjectListException(message);
                    }
                }
            }
        }

        private void CreateProxy(IServerRoDbClass parentRoEntity, Type type, IDbConnection con, IDbRelation relation,
                                 object value, PropertyInfo property)
        {
            Type proxyType = value == null ? property.PropertyType : value.GetType();
            if (proxyType.IsGenericType)
            {
                Type generic = proxyType.GetGenericArguments()[0];
                if (proxyType.IsInterface)
                {
                    proxyType = typeof(List<>).MakeGenericType(new Type[] { generic });
                }
            }
            if (value == null)
            {
                value = Activator.CreateInstance(proxyType);
            }

            Object proxy = null;
            if (ReflectionUtils.IsImplementInterface(property.PropertyType, typeof (ICollection<>)))
            {
                Type generic = proxyType.GetGenericArguments()[0];
                Type genericType = typeof (ICollection<>).MakeGenericType(new Type[] {generic});
                proxy = _proxyGenerator.CreateInterfaceProxyWithTarget(genericType, value,
                                                                       new ChildLoadInterceptor(this, parentRoEntity, type, con,
                                                                                                relation));
            }
            else
            {
                proxy = _proxyGenerator.CreateClassProxy(proxyType, new object[] {},
                                                         new ChildLoadInterceptor(this, parentRoEntity, type, con, relation));
            }
            property.SetValue(parentRoEntity, proxy, new object[] {});
        }
    }
}
