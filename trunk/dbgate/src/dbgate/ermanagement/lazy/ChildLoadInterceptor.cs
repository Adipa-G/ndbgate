using System;
using System.Data;
using System.Reflection;
using Castle.DynamicProxy;
using dbgate.dbutility;
using dbgate.ermanagement.caches;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement.lazy
{
    public class ChildLoadInterceptor : IInterceptor
    {
        private ErDataRetrievalManager _dataRetrievalManager;
        private IServerRoDbClass _parentRoEntity;
        private Type _applicableParentType;
        private IDbConnection _connection;
        private IDbRelation _relation;

        private bool _intercepted;

        public ChildLoadInterceptor(ErDataRetrievalManager dataRetrievalManager, IServerRoDbClass parentRoEntity
            , Type applicableParentType, IDbConnection connection, IDbRelation relation)
        {
            _dataRetrievalManager = dataRetrievalManager;
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
                    _dataRetrievalManager.LoadChildrenFromRelation(_parentRoEntity, _applicableParentType, _connection, _relation, true);
                }
                finally
                {
                    if (newConnection)
                    {
                        DbMgmtUtility.Close(_connection);
                        _connection = null;
                    }
                }

                PropertyInfo property = CacheManager.MethodCache.GetProperty(_parentRoEntity.GetType(), _relation.AttributeName);
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
