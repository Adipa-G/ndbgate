using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.Context;
using DbGate.ErManagement.DbAbstractionLayer;
using DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.Selection;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.ErManagement.Lazy;
using DbGate.ErManagement.Query;
using DbGate.Exceptions;
using DbGate.Exceptions.Common;
using DbGate.Exceptions.Retrival;
using DbGate.Utility;
using log4net;

namespace DbGate.ErManagement.ErMapper
{
    public class RetrievalOperationLayer : BaseOperationLayer
    {
        private ProxyGenerator _proxyGenerator;

        public RetrievalOperationLayer(IDbLayer dbLayer,IDbGateStatistics statistics, IDbGateConfig config)
            : base(dbLayer,statistics, config)
        {
            _proxyGenerator = new ProxyGenerator();
        }

		public ICollection<Object> Select (ISelectionQuery query, ITransaction tx)
		{
			IDataReader rs = null;
			try 
			{
				var logSb = new StringBuilder ();
				var showQuery = Config.ShowQueries;
				var buildInfo = DbLayer.DataManipulate ().ProcessQuery (null,query.Structure);
				var execInfo = buildInfo.ExecInfo;
				
				if (showQuery) 
				{
					logSb.Append (execInfo.Sql);
					foreach (var param in execInfo.Params) 
					{
						logSb.Append (" ,").Append ("Param").Append (param.Index).Append ("=").Append (param.Value);
					}
					Logger.GetLogger(Config.LoggerName).Debug(logSb.ToString());
				}
		
				rs = DbLayer.DataManipulate().CreateResultSet(tx, execInfo);

                IList<Object> retList = new List<Object> ();
				ICollection<IQuerySelection> selections = query.Structure.SelectList;
                var selectionCount = selections.Count;

				while (rs.Read()) 
				{
					int count = 0;
				    
				    object rowObject = selectionCount > 1 ? new object[selectionCount] : null;
					foreach (IQuerySelection selection in selections) 
					{
						Object loaded = ((IAbstractSelection)selection).Retrieve (rs,tx,buildInfo);
                        if (selectionCount > 1)
                        {
                            ((object[])rowObject)[count++] = loaded;
                        }
                        else
                        {
                            rowObject = loaded;
                        }
					}
                    retList.Add(rowObject);
				}
		
				return retList;
			} 
			catch (Exception e) 
			{
				Logger.GetLogger(Config.LoggerName).Error(e.Message, e);
				throw new RetrievalException (e.Message, e);
			} 
			finally 
			{
				DbMgtUtility.Close (rs);
			}
		}

        public void Load(IReadOnlyEntity roEntity, IDataReader reader, ITransaction tx)
        {
            if (roEntity is IEntity)
            {
                IEntity entity = (IEntity)roEntity;
                entity.Status = EntityStatus.Unmodified;
            }
            try
            {
                LoadFromDb(roEntity, reader, tx);
                roEntity.Context.DestroyReferenceStore();
            }
            catch (Exception e)
            {
                Logger.GetLogger( Config.LoggerName).Fatal(e.Message, e);
                throw new RetrievalException(e.Message, e);
            }
        }

        private void LoadFromDb(IReadOnlyEntity roEntity, IDataReader reader, ITransaction tx)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(roEntity);
            while (entityInfo != null)
            {
                string tableName = entityInfo.TableInfo.TableName;
                if (entityInfo.EntityType == roEntity.GetType() || tableName == null) //if i==0 that means it's base class and can use existing result set
                {
                    LoadForType(roEntity, entityInfo.EntityType, reader, tx);
                }
                else
                {
                    IDbCommand superCmd = null;
                    IDataReader superReader = null;
                    try
                    {
                        ITypeFieldValueList keyValueList = OperationUtils.ExtractEntityTypeKeyValues(roEntity, entityInfo.EntityType);
                        superCmd = CreateRetrievalPreparedStatement(keyValueList, tx);
                        superReader = superCmd.ExecuteReader();
                        if (superReader.Read())
                        {
                            LoadForType(roEntity, entityInfo.EntityType, superReader, tx);
                        }
                        else
                        {
                            string message =
                                String.Format(
                                    "Super class {0} does not contains a matching record for the base class {1}",
                                    entityInfo.EntityType.FullName, roEntity.GetType().FullName);
                            throw new NoMatchingRecordFoundForSuperClassException(message);
                        }
                    }
                    catch(Exception ex)
                    {
                        String message = String.Format("SQL Exception while trying to read from table {0}",tableName);
                        throw new ReadFromResultSetException(message,ex);
                    }
                    finally
                    {
                        DbMgtUtility.Close(superReader);
                        DbMgtUtility.Close(superCmd);
                    }
                }
                entityInfo = entityInfo.SuperEntityInfo;
            }
        }

        private void LoadForType(IReadOnlyEntity entity, Type type, IDataReader reader, ITransaction tx)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(type);
            IEntityContext entityContext = entity.Context;
            ITypeFieldValueList valueTypeList = ReadValues(type, reader);
            SetValues(entity, valueTypeList);
            entity.Context.AddToCurrentObjectGraphIndex(entity);

            if (entityContext != null)
            {
                entityContext.ChangeTracker.AddFields(valueTypeList.FieldValues);
            }

            ICollection<IRelation> dbRelations = entityInfo.Relations;
            foreach (IRelation relation in dbRelations)
            {
                LoadChildrenFromRelation(entity, type, tx, relation,false);
            }
        }

        public void LoadChildrenFromRelation(IReadOnlyEntity parentRoEntity, Type entityType, ITransaction tx
            , IRelation relation,bool lazy)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(entityType);
            IEntityContext entityContext = parentRoEntity.Context;

            PropertyInfo property = entityInfo.GetProperty(relation.AttributeName);
            Object value = ReflectionUtils.GetValue(property,parentRoEntity);
            
            if (!lazy && relation.FetchStrategy == FetchStrategy.Lazy)
            {
                CreateProxy(parentRoEntity, entityType, tx, relation, value, property);
                return;
            }

            ICollection<IReadOnlyEntity> children = ReadRelationChildrenFromDb(parentRoEntity, entityType, tx, relation);
            if (entityContext != null
                    && !relation.ReverseRelationship)
            {
                foreach (IReadOnlyEntity childEntity in children)
                {
                    ITypeFieldValueList valueTypeList = OperationUtils.ExtractRelationKeyValues(childEntity, relation);
                    if (valueTypeList != null)
                    {
                        entityContext.ChangeTracker.AddChildEntityKey(valueTypeList);
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
                foreach (IReadOnlyEntity serverRoDbClass in children)
                {
                    genCollection.Add(serverRoDbClass);
                }
                ReflectionUtils.SetValue(property,parentRoEntity,genCollection);
            }
            else if (value != null
                    && ReflectionUtils.IsImplementInterface(property.PropertyType, typeof(ICollection<>)))
            {
                IList genCollection = (IList)value;
                foreach (IReadOnlyEntity serverRoDbClass in children)
                {
                    genCollection.Add(serverRoDbClass);
                }
            }
            else
            {
                IEnumerator<IReadOnlyEntity> childEnumarator = children.GetEnumerator();
                if (childEnumarator.MoveNext())
                {
                    IReadOnlyEntity singleRoDbClass = childEnumarator.Current;
                    if (property.PropertyType.IsAssignableFrom(singleRoDbClass.GetType()))
                    {
                        ReflectionUtils.SetValue(property,parentRoEntity,singleRoDbClass);
                    }
                    else
                    {
                        string message = singleRoDbClass.GetType().FullName + " is not matching the getter " + property.Name;
                        Logger.GetLogger( Config.LoggerName).Fatal(message);
                        throw new NoSetterFoundToSetChildObjectListException(message);
                    }
                }
            }
        }

        private void CreateProxy(IReadOnlyEntity parentRoEntity, Type type, ITransaction tx, IRelation relation,
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
                                                                       new ChildLoadInterceptor(this, parentRoEntity, type, tx,
                                                                                                relation));
            }
            else
            {
                proxy = _proxyGenerator.CreateClassProxy(proxyType, new object[] {},
                                                         new ChildLoadInterceptor(this, parentRoEntity, type, tx, relation));
            }
            ReflectionUtils.SetValue(property,parentRoEntity,proxy);
        }
    }
}
