using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Exceptions;
using DbGate.Patch.Support.PatchEmpty;
using log4net;
using Xunit;

namespace DbGate.Patch
{
    [Collection("Sequential")]
    public class DbGatePatchEmptyDbTests : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-metadata-empty";

        public DbGatePatchEmptyDbTests()
        {
            TestClass = typeof(DbGatePatchEmptyDbTests);
            BeginInit(DbName);
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
            
            ICollection<Type> types = new List<Type>();
            types.Add(typeof(LeafEntitySubA));
            types.Add(typeof(LeafEntitySubB));
            types.Add(typeof(RootEntity));

            var transaction = CreateTransaction();
            transaction.DbGate.PatchDataBase(transaction, types, true);
            transaction.Commit();
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }

        [Fact]
        public void PatchEmpty_PatchDataBase_WithEmptyDb_ShouldCreateTables_ShouldBeAbleToInsertData()
        {
            try
            {
                var transaction = CreateTransaction();

                var id = 36;
                var entity = CreateRootEntityWithoutNullValues(id);
                entity.LeafEntities.Add(CreateLeafEntityA(id, 1));
                entity.LeafEntities.Add(CreateLeafEntityB(id, 2));
                entity.Persist(transaction);
                transaction.Commit();
 
                transaction = CreateTransaction();
                var loadedEntity = LoadRootEntityWithId(transaction, id);
                transaction.Commit();

                AssertTwoRootEntitiesEquals(entity, loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }


        [Fact]
        public void PatchEmpty_PatchDataBase_WithEmptyDb_ShouldCreatePrimaryKeys_ShouldNotAbleToPutDuplicateData()
        {
            var transaction = CreateTransaction();

            var id = 37;
            var entity = CreateRootEntityWithoutNullValues(id);
            entity.LeafEntities.Add(CreateLeafEntityA(id, 1));
            entity.LeafEntities.Add(CreateLeafEntityB(id, 1));
            Assert.Throws<PersistException>(() => entity.Persist(transaction));

            transaction.Commit();
            transaction.Close();
        }

        [Fact]
        public void PatchEmpty_PatchDataBase_WithEmptyDb_ShouldCreateForeignKeys_ShouldNotAbleToInconsistantData()
        {
            var transaction = CreateTransaction();

            var id = 38;
            var entity = CreateRootEntityWithoutNullValues(id);
            entity.Persist(transaction);

            var leafEntityA = CreateLeafEntityA(id, 1);
            leafEntityA.Persist(transaction);

            var leafEntityB = CreateLeafEntityA(id + 1, 1);
            Assert.Throws<PersistException>(() => leafEntityB.Persist(transaction));

            transaction.Commit();
            transaction.Close();
        }

        [Fact]
        public void PatchEmpty_PatchDataBase_PatchTwice_ShouldNotThrowException()
        {
            var transaction = CreateTransaction();

            ICollection<Type> types = new List<Type>();
            types.Add(typeof(LeafEntitySubA));
            types.Add(typeof(LeafEntitySubB));
            types.Add(typeof(RootEntity));

            transaction.DbGate.PatchDataBase(transaction, types, true);
            transaction.Commit();
        }

        private void AssertTwoRootEntitiesEquals(RootEntity entityA, RootEntity entityB)
        {
            Assert.Equal(entityA.IdCol, entityB.IdCol);

            Assert.Equal(entityA.CharNotNull, entityB.CharNotNull);
            Assert.Equal(entityA.CharNull, entityB.CharNull);
            Assert.Equal(entityA.DateNotNull, entityB.DateNotNull);
            Assert.Equal(entityA.DateNull, entityB.DateNull);
            Assert.Equal(entityA.DoubleNotNull, entityB.DoubleNotNull);
            Assert.Equal(entityA.DoubleNull, entityB.DoubleNull);
            Assert.Equal(entityA.FloatNotNull, entityB.FloatNotNull);
            Assert.Equal(entityA.FloatNull, entityB.FloatNull);
            Assert.Equal(entityA.IntNotNull, entityB.IntNotNull);
            Assert.Equal(entityA.IntNull, entityB.IntNull);
            Assert.Equal(entityA.LongNotNull, entityB.LongNotNull);
            Assert.Equal(entityA.LongNull, entityB.LongNull);
            Assert.Equal(entityA.TimestampNotNull, entityB.TimestampNotNull);
            Assert.Equal(entityA.TimestampNull, entityB.TimestampNull);
            Assert.Equal(entityA.VarcharNotNull, entityB.VarcharNotNull);
            Assert.Equal(entityA.VarcharNull, entityB.VarcharNull);
            Assert.Equal(entityA.LeafEntities.Count, entityB.LeafEntities.Count);

            var iteratorA = entityA.LeafEntities.GetEnumerator();

            while (iteratorA.MoveNext())
            {
                var leafEntityA = iteratorA.Current;
                var bFound = false;
                foreach (var leafEntityB in entityB.LeafEntities)
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
            Assert.Equal(entityA.IdCol, entityB.IdCol);
            Assert.Equal(entityA.IndexNo, entityB.IndexNo);
            Assert.Equal(entityA.SomeText, entityB.SomeText);
            if (entityA is LeafEntitySubA && entityB is LeafEntitySubA)
            {
                Assert.Equal(((LeafEntitySubA)entityA).SomeTextA, ((LeafEntitySubA)entityB).SomeTextA);
            }
            if (entityA is LeafEntitySubB && entityB is LeafEntitySubB)
            {
                Assert.Equal(((LeafEntitySubB)entityA).SomeTextB, ((LeafEntitySubB)entityB).SomeTextB);
            }
        }

        private RootEntity LoadRootEntityWithId(ITransaction transaction, int id)
        {
            RootEntity loadedEntity = null;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from root_entity where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new RootEntity();
                loadedEntity.Retrieve(rs, transaction);
            }

            return loadedEntity;
        }

        private LeafEntity CreateLeafEntityA(int id, int index)
        {
            var leafEntity = new LeafEntitySubA();
            leafEntity.IdCol = id;
            leafEntity.IndexNo = index;
            leafEntity.SomeTextA = "text A";
            leafEntity.SomeText = "Id : " + id + " - " + " Index : " + index;

            return leafEntity;
        }

        private LeafEntity CreateLeafEntityB(int id, int index)
        {
            var leafEntity = new LeafEntitySubB();
            leafEntity.IdCol = id;
            leafEntity.IndexNo = index;
            leafEntity.SomeTextB = "text B";
            leafEntity.SomeText = "Id : " + id + " - " + " Index : " + index;

            return leafEntity;
        }

        private RootEntity CreateRootEntityWithoutNullValues(int id)
        {
            var entity = new RootEntity();
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