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
        private readonly Type applicableParentType;
        private readonly RetrievalOperationLayer dataRetrievalOperationLayer;
        private readonly IReadOnlyEntity parentRoEntity;
        private readonly IRelation relation;
        private readonly ITransactionFactory transactionFactory;
        private ITransaction transaction;

        private bool intercepted;

        public ChildLoadInterceptor(RetrievalOperationLayer dataRetrievalOperationLayer, IReadOnlyEntity parentRoEntity
                                    , Type applicableParentType, ITransaction transaction, IRelation relation)
        {
            this.dataRetrievalOperationLayer = dataRetrievalOperationLayer;
            this.parentRoEntity = parentRoEntity;
            this.applicableParentType = applicableParentType;
            this.transaction = transaction;
            transactionFactory = transaction.Factory;
            this.relation = relation;
        }

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            if (!intercepted)
            {
                intercepted = true;
                var newTransaction = false;
                try
                {
                    if (transaction.Closed)
                    {
                        transaction = transactionFactory.CreateTransaction();
                        newTransaction = true;
                    }
                    dataRetrievalOperationLayer.LoadChildrenFromRelation(parentRoEntity, applicableParentType,
                                                                          transaction, relation, true);
                }
                finally
                {
                    if (newTransaction)
                    {
                        DbMgtUtility.Close(transaction);
                        transaction = null;
                    }
                }

                var entityInfo = CacheManager.GetEntityInfo(parentRoEntity);
                var property = entityInfo.GetProperty(relation.AttributeName);
                var objectToInvoke = property.GetValue(parentRoEntity, new object[] {});

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