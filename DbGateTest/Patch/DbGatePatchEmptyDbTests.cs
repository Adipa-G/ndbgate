using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Exceptions;
using DbGate.Patch.Support.PatchEmpty;
using log4net;
using NUnit.Framework;

namespace DbGate.Patch
{
    [TestFixture]
    public class DbGatePatchEmptyDbTests : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-metadata-empty";

        [OneTimeSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGatePatchEmptyDbTests);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
            
            ICollection<Type> types = new List<Type>();
            types.Add(typeof(LeafEntitySubA));
            types.Add(typeof(LeafEntitySubB));
            types.Add(typeof(RootEntity));

            ITransaction transaction = CreateTransaction();
            transaction.DbGate.PatchDataBase(transaction, types, true);
            transaction.Commit();
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }

        [Test]
        public void PatchEmpty_PatchDataBase_WithEmptyDb_ShouldCreateTables_ShouldBeAbleToInsertData()
        {
            try
            {
                ITransaction transaction = CreateTransaction();

                int id = 36;
                RootEntity entity = CreateRootEntityWithoutNullValues(id);
                entity.LeafEntities.Add(CreateLeafEntityA(id, 1));
                entity.LeafEntities.Add(CreateLeafEntityB(id, 2));
                entity.Persist(transaction);
                transaction.Commit();
 
                transaction = CreateTransaction();
                RootEntity loadedEntity = LoadRootEntityWithId(transaction, id);
                transaction.Commit();

                AssertTwoRootEntitiesEquals(entity, loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void PatchEmpty_PatchDataBase_WithEmptyDb_ShouldCreatePrimaryKeys_ShouldNotAbleToPutDuplicateData()
        {
            ITransaction transaction = CreateTransaction();

            int id = 37;
            RootEntity entity = CreateRootEntityWithoutNullValues(id);
            entity.LeafEntities.Add(CreateLeafEntityA(id, 1));
            entity.LeafEntities.Add(CreateLeafEntityB(id, 1));
            Assert.Throws<PersistException>(() => entity.Persist(transaction));

            transaction.Commit();
            transaction.Close();
        }

        [Test]
        public void PatchEmpty_PatchDataBase_WithEmptyDb_ShouldCreateForeignKeys_ShouldNotAbleToInconsistantData()
        {
            ITransaction transaction = CreateTransaction();

            int id = 38;
            RootEntity entity = CreateRootEntityWithoutNullValues(id);
            entity.Persist(transaction);

            var leafEntityA = CreateLeafEntityA(id, 1);
            leafEntityA.Persist(transaction);

            var leafEntityB = CreateLeafEntityA(id + 1, 1);
            Assert.Throws<PersistException>(() => leafEntityB.Persist(transaction));

            transaction.Commit();
            transaction.Close();
        }

        [Test]
        public void PatchEmpty_PatchDataBase_PatchTwice_ShouldNotThrowException()
        {
            ITransaction transaction = CreateTransaction();

            ICollection<Type> types = new List<Type>();
            types.Add(typeof(LeafEntitySubA));
            types.Add(typeof(LeafEntitySubB));
            types.Add(typeof(RootEntity));

            transaction.DbGate.PatchDataBase(transaction, types, true);
            transaction.Commit();
        }

        private void AssertTwoRootEntitiesEquals(RootEntity entityA, RootEntity entityB)
        {
            Assert.AreEqual(entityA.IdCol, entityB.IdCol);

            Assert.AreEqual(entityA.CharNotNull, entityB.CharNotNull);
            Assert.AreEqual(entityA.CharNull, entityB.CharNull);
            Assert.AreEqual(entityA.DateNotNull, entityB.DateNotNull);
            Assert.AreEqual(entityA.DateNull, entityB.DateNull);
            Assert.AreEqual(entityA.DoubleNotNull, entityB.DoubleNotNull);
            Assert.AreEqual(entityA.DoubleNull, entityB.DoubleNull);
            Assert.AreEqual(entityA.FloatNotNull, entityB.FloatNotNull);
            Assert.AreEqual(entityA.FloatNull, entityB.FloatNull);
            Assert.AreEqual(entityA.IntNotNull, entityB.IntNotNull);
            Assert.AreEqual(entityA.IntNull, entityB.IntNull);
            Assert.AreEqual(entityA.LongNotNull, entityB.LongNotNull);
            Assert.AreEqual(entityA.LongNull, entityB.LongNull);
            Assert.AreEqual(entityA.TimestampNotNull, entityB.TimestampNotNull);
            Assert.AreEqual(entityA.TimestampNull, entityB.TimestampNull);
            Assert.AreEqual(entityA.VarcharNotNull, entityB.VarcharNotNull);
            Assert.AreEqual(entityA.VarcharNull, entityB.VarcharNull);
            Assert.AreEqual(entityA.LeafEntities.Count, entityB.LeafEntities.Count);

            IEnumerator<LeafEntity> iteratorA = entityA.LeafEntities.GetEnumerator();

            while (iteratorA.MoveNext())
            {
                LeafEntity leafEntityA = iteratorA.Current;
                bool bFound = false;
                foreach (LeafEntity leafEntityB in entityB.LeafEntities)
                {
                    if (leafEntityB.IdCol == leafEntityA.IdCol
                        && leafEntityB.IndexNo == leafEntityA.IndexNo)
                    {
                        bFound = true;
                        AssertTwoLeafEntitiesTypeAEquals(leafEntityA, leafEntityB);
                    }
                }
                if (!bFound)
                {
                    Assert.Fail("Could not found matching leaf entity");
                }
            }
        }

        private void AssertTwoLeafEntitiesTypeAEquals(LeafEntity entityA, LeafEntity entityB)
        {
            Assert.AreEqual(entityA.IdCol, entityB.IdCol);
            Assert.AreEqual(entityA.IndexNo, entityB.IndexNo);
            Assert.AreEqual(entityA.SomeText, entityB.SomeText);
            if (entityA is LeafEntitySubA && entityB is LeafEntitySubA)
            {
                Assert.AreEqual(((LeafEntitySubA)entityA).SomeTextA, ((LeafEntitySubA)entityB).SomeTextA);
            }
            if (entityA is LeafEntitySubB && entityB is LeafEntitySubB)
            {
                Assert.AreEqual(((LeafEntitySubB)entityA).SomeTextB, ((LeafEntitySubB)entityB).SomeTextB);
            }
        }

        private RootEntity LoadRootEntityWithId(ITransaction transaction, int id)
        {
            RootEntity loadedEntity = null;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from root_entity where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new RootEntity();
                loadedEntity.Retrieve(rs, transaction);
            }

            return loadedEntity;
        }

        private LeafEntity CreateLeafEntityA(int id, int index)
        {
            LeafEntitySubA leafEntity = new LeafEntitySubA();
            leafEntity.IdCol = id;
            leafEntity.IndexNo = index;
            leafEntity.SomeTextA = "text A";
            leafEntity.SomeText = "Id : " + id + " - " + " Index : " + index;

            return leafEntity;
        }

        private LeafEntity CreateLeafEntityB(int id, int index)
        {
            LeafEntitySubB leafEntity = new LeafEntitySubB();
            leafEntity.IdCol = id;
            leafEntity.IndexNo = index;
            leafEntity.SomeTextB = "text B";
            leafEntity.SomeText = "Id : " + id + " - " + " Index : " + index;

            return leafEntity;
        }

        private RootEntity CreateRootEntityWithoutNullValues(int id)
        {
            RootEntity entity = new RootEntity();
            entity.IdCol = id;

            entity.BooleanNotNull = true;
            entity.BooleanNull = true;
            entity.CharNotNull = 'A';
            entity.CharNull = 'B';
            entity.DateNotNull = DateTime.Now.AddYears(1);
            entity.DateNull = DateTime.Now.AddMonths(1);
            entity.DoubleNotNull = 5D;
            entity.DoubleNull = 6D;
            entity.FloatNotNull = 20F;
            entity.FloatNull = 20F;
            entity.IntNotNull = 24;
            entity.IntNull = 23;
            entity.LongNotNull = 356L;
            entity.LongNull = 326L;
            entity.TimestampNotNull = DateTime.Now.AddHours(1);
            entity.TimestampNull = DateTime.Now.AddMinutes(1);
            entity.VarcharNotNull = "notNull";
            entity.VarcharNull = "null";

            return entity;
        }
    }
}