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
        private readonly ITransactionFactory _transactionFactory;
        private ITransaction _transaction;

        private bool _intercepted;

        public ChildLoadInterceptor(RetrievalOperationLayer dataRetrievalOperationLayer, IReadOnlyEntity parentRoEntity
                                    , Type applicableParentType, ITransaction transaction, IRelation relation)
        {
            _dataRetrievalOperationLayer = dataRetrievalOperationLayer;
            _parentRoEntity = parentRoEntity;
            _applicableParentType = applicableParentType;
            _transaction = transaction;
            _transactionFactory = transaction.Factory;
            _relation = relation;
        }

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            if (!_intercepted)
            {
                _intercepted = true;
                bool newTransaction = false;
                try
                {
                    if (_transaction.Closed)
                    {
                        _transaction = _transactionFactory.CreateTransaction();
                        newTransaction = true;
                    }
                    _dataRetrievalOperationLayer.LoadChildrenFromRelation(_parentRoEntity, _applicableParentType,
                                                                          _transaction, _relation, true);
                }
                finally
                {
                    if (newTransaction)
                    {
                        DbMgtUtility.Close(_transaction);
                        _transaction = null;
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