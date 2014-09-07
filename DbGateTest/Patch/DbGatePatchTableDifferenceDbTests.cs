using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Patch.Support.PatchTableDifferences;
using log4net;
using NUnit.Framework;

namespace DbGate.Patch
{
    public class DbGatePatchTableDifferenceDbTests : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-metadata-table-difference";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGatePatchTableDifferenceDbTests);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnAdded_ShouldAddColumn()
        {
            try
            {
                ITransaction transaction = TransactionFactory.CreateTransaction();
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                TransactionFactory.DbGate.PatchDataBase(transaction, types, true);
                transaction.Commit();

                transaction = CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (FourColumnEntity));
                TransactionFactory.DbGate.PatchDataBase(transaction, types, false);
                transaction.Commit();

                int id = 35;
                transaction = CreateTransaction(); 
                FourColumnEntity columnEntity = CreateFourColumnEntity(id);
                columnEntity.Persist(transaction);
                columnEntity = LoadFourColumnEntityWithId(transaction, id);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnDeleted_ShouldDeleteColumn()
        {
            try
            {
                ITransaction transaction = TransactionFactory.CreateTransaction(); 
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (FourColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, true);
                transaction.Commit();

                transaction = CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, false);
                transaction.Commit();
 
                //Sqllite does not support dropping columns, so this test does not work
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnChanged_ShouldUpdateColumn()
        {
            try
            {
                var longStr = new string('A', 220);

                ITransaction transaction = TransactionFactory.CreateTransaction(); 
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, true);
                transaction.Commit();
 
                int id = 34;
                transaction = CreateTransaction(); 
                ThreeColumnEntity columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                try
                {
                    columnEntity.Persist(transaction);
                }
                catch (AssertionException)
                {
                    throw;
                }
                catch (System.Exception)
                {
                }

                transaction = CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (ThreeColumnTypeDifferentEntity));
                transaction.DbGate.PatchDataBase(transaction, types, false);
                transaction.Commit();
 
                id = 35;
                transaction = CreateTransaction();  
                columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                columnEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private FourColumnEntity LoadFourColumnEntityWithId(ITransaction transaction, int id)
        {
            FourColumnEntity loadedEntity = null;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from table_change_test_entity where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new FourColumnEntity();
                loadedEntity.Retrieve(rs, transaction);
            }

            return loadedEntity;
        }

        private FourColumnEntity CreateFourColumnEntity(int id)
        {
            var entity = new FourColumnEntity();
            entity.IdCol = id;
            entity.Code = "4C";
            entity.Name = "4Col";
            entity.IndexNo = 0;
            return entity;
        }

        private ThreeColumnEntity CreateThreeColumnEntity(int id)
        {
            var entity = new ThreeColumnEntity();
            entity.IdCol = id;
            entity.Name = "3Col";
            entity.IndexNo = 0;
            return entity;
        }
    }
}