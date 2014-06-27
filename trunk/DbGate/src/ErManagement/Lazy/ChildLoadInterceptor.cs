using System;
using System.Data;
using System.Reflection;
using Castle.DynamicProxy;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.ErManagement.ErMapper;
using DbGate.Utility;

namespace DbGate.ErManagement.Lazy
{
    public class ChildLoadInterceptor : IInterceptor
    {
        private readonly Type _applicableParentType;
        private readonly RetrievalOperationLayer _dataRetrievalOperationLayer;
        private readonly IReadOnlyEntity _parentRoEntity;
        private readonly IRelation _relation;
        private IDbConnection _connection;

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

        #region IInterceptor Members

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
                    _dataRetrievalOperationLayer.LoadChildrenFromRelation(_parentRoEntity, _applicableParentType,
                                                                          _connection, _relation, true);
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
                Object objectToInvoke = property.GetValue(_parentRoEntity, new object[] {});

                invocation.ReturnValue = invocation.Method.Invoke(objectToInvoke, new object[] {});
            }
            else
            {
                invocation.Proceed();
            }
        }

        #endregion
    }
}