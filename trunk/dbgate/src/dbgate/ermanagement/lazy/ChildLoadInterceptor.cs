using System;
using System.Data;
using System.Reflection;
using Castle.DynamicProxy;
using dbgate.caches;
using dbgate.caches.impl;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.utility;

namespace dbgate.ermanagement.lazy
{
    public class ChildLoadInterceptor : IInterceptor
    {
        private RetrievalOperationLayer _dataRetrievalOperationLayer;
        private IReadOnlyEntity _parentRoEntity;
        private Type _applicableParentType;
        private IDbConnection _connection;
        private IRelation _relation;

        private bool _intercepted;

        public ChildLoadInterceptor(RetrievalOperationLayer dataRetrievalOperationLayer, IReadOnlyEntity parentRoEntity
            , Type applicableParentType, IDbConnection connection, IRelation relation)
        {
            _dataRetrievalOperationLayer = dataRetrievalOperationLayer;
            _parentRoEntity = parentRoEntity;
            _applicableParentType = applicableParentType;
            _connection = connection;
            _relation = relation;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!_intercepted)
            {
                _intercepted = true;
                bool newConnection = false;
                try
                {
                    if (_connection == null || _connection.State != ConnectionState.Open)
                    {
                        _connection = DbConnector.GetSharedInstance().Connection;
                        newConnection = true;
                    }
                    _dataRetrievalOperationLayer.LoadChildrenFromRelation(_parentRoEntity, _applicableParentType, _connection, _relation, true);
                }
                finally
                {
                    if (newConnection)
                    {
                        DbMgtUtility.Close(_connection);
                        _connection = null;
                    }
                }

                EntityInfo entityInfo = CacheManager.GetEntityInfo(_parentRoEntity);
                PropertyInfo property = entityInfo.GetProperty(_relation.AttributeName);
                Object objectToInvoke = property.GetValue(_parentRoEntity,new object[]{});

                invocation.ReturnValue =  invocation.Method.Invoke(objectToInvoke, new object[] {});
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
