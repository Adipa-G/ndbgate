using System;
using System.Data;
using System.Reflection;
using DbGate.Caches.Impl;
using DbGate.ErManagement.ErMapper;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.Exception.Support;
using DbGate.Exceptions.Common;
using Xunit;

namespace DbGate.Exception
{
    [Collection("Sequential")]
    public class DbGateExceptionTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-exceptions";

        public DbGateExceptionTest()
        {
            TestClass = typeof(DbGateExceptionTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName); 
        }

        private IDbConnection SetupTables()
        {
            var sql = "Create table exception_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);
            EndInit(DbName);

            return Connection;
        }

        [Fact]
        public void EntityInstantiationException_EntityWithoutDefaultConstructor_ShouldFail()
        {
            try
            {
                ReflectionUtils.CreateInstance(typeof(EntityWithAllWrong));
                Assert.Fail("could create instance without default constructor");
            }
            catch (EntityInstantiationException)
            {
                Assert.True(true,"could not create instance without default constructor");
            }
        }

        [Fact]
        public void EntityRegistrationException_EntityWithoutDefaultConstructor_ShouldFail()
        {
            try
            {
                var cache = new EntityInfoCache(new DbGateConfig());
                cache.Register(typeof(EntityWithAllWrong));
                Assert.Fail("could register class without default constructor");
            }
            catch (EntityRegistrationException)
            {
                Assert.True(true,"could not register class without default constructor");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }

        [Fact]
        public void MethodInvocationException_EntityWithExceptionsWhenGetterInvoked_ShouldFail()
        {
            try
            {
                var entity = new EntityWithAllWrong(123);
                var property = entity.GetType().GetProperty("IdCol");
                ReflectionUtils.GetValue(entity.GetType(), property.Name, entity);
                Assert.Fail("could invoke getter method");
            }
            catch (MethodInvocationException)
            {
                Assert.True(true,"could not invoke getter method");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }

        [Fact]
        public void MethodInvocationException_EntityWithExceptionsWhenSetterInvoked_ShouldFail()
        {
            try
            {
                var entity = new EntityWithAllWrong(123);
                var property = entity.GetType().GetProperty("IdCol");
                ReflectionUtils.SetValue(entity.GetType(), property.Name, entity, 0);
                Assert.Fail("could invoke setter method");
            }
            catch (MethodInvocationException)
            {
                Assert.True(true,"could not invoke setter method");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }

        [Fact]
        public void MethodNotFoundException_GetInfoOnNonExistentMethod_ShouldFail()
        {
            try
            {
                var info = new EntityInfo(typeof(EntityWithAllWrong));
                info.GetProperty("nonExistent");
                Assert.Fail("could get method");
            }
            catch (PropertyNotFoundException)
            {
                Assert.True(true,"could not get method");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }
    }
}