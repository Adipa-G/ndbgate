using System.Data;
using System.Reflection;
using DbGate.Caches.Impl;
using DbGate.ErManagement.ErMapper;
using DbGate.ErManagement.ErMapper.Utils;
using DbGate.Exception.Support;
using DbGate.Exceptions.Common;
using NUnit.Framework;

namespace DbGate.Exception
{
    [TestFixture]
    public class DbGateExceptionTest : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-exceptions";

        [OneTimeSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateExceptionTest);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }

        private IDbConnection SetupTables()
        {
            string sql = "Create table exception_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);
            EndInit(DBName);

            return Connection;
        }

        [Test]
        public void EntityInstantiationException_EntityWithoutDefaultConstructor_ShouldFail()
        {
            try
            {
                ReflectionUtils.CreateInstance(typeof(EntityWithAllWrong));
                Assert.Fail("could create instance without default constructor");
            }
            catch (EntityInstantiationException)
            {
                Assert.IsTrue(true,"could not create instance without default constructor");
            }
        }

        [Test]
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
                Assert.IsTrue(true,"could not register class without default constructor");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }

        [Test]
        public void MethodInvocationException_EntityWithExceptionsWhenGetterInvoked_ShouldFail()
        {
            try
            {
                var entity = new EntityWithAllWrong(123);
                PropertyInfo property = entity.GetType().GetProperty("IdCol");
                ReflectionUtils.GetValue(property, entity);
                Assert.Fail("could invoke getter method");
            }
            catch (MethodInvocationException)
            {
                Assert.IsTrue(true,"could not invoke getter method");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }

        [Test]
        public void MethodInvocationException_EntityWithExceptionsWhenSetterInvoked_ShouldFail()
        {
            try
            {
                var entity = new EntityWithAllWrong(123);
                PropertyInfo property = entity.GetType().GetProperty("IdCol");
                ReflectionUtils.SetValue(property, entity, 0);
                Assert.Fail("could invoke setter method");
            }
            catch (MethodInvocationException)
            {
                Assert.IsTrue(true,"could not invoke setter method");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }

        [Test]
        public void MethodNotFoundException_GetInfoOnNonExistentMethod_ShouldFail()
        {
            try
            {
                EntityInfo info = new EntityInfo(typeof(EntityWithAllWrong));
                info.GetProperty("nonExistent");
                Assert.Fail("could get method");
            }
            catch (PropertyNotFoundException)
            {
                Assert.IsTrue(true,"could not get method");
            }
            catch (System.Exception e)
            {
                Assert.Fail("unexpected exception"  + e.Message);
            }
        }
    }
}