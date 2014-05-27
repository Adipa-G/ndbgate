using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using dbgate.dbutility;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.patch.patchempty;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementPatchEmptyDbTests
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (ErManagementPatchEmptyDbTests)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=4;foreign_keys = ON", DbConnector.DbSqllite);

                IDbConnection connection = dbConnector.Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementPatchEmptyDbTests)).Fatal("Exception during database startup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;

                ICollection<IServerDbClass> dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new LeafEntitySubA());
                dbClasses.Add(new LeafEntitySubB());
                dbClasses.Add(new RootEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses, true);

                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementPatchEmptyDbTests)).Fatal("Exception during test initialization.", ex);
            }
        }

        [TestFixtureTearDown]
        public static void After()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementPatchEmptyDbTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [Test]
        public void ERLayer_patchDataBase_withEmptyDb_shouldCreateTables_shouldBeAbleToInsertData()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                int id = 36;
                RootEntity entity = CreateRootEntityWithoutNullValues(id);
                entity.LeafEntities.Add(CreateLeafEntityA(id,1));
                entity.LeafEntities.Add(CreateLeafEntityB(id,2));
                entity.Persist(connection);

                transaction.Commit();
                connection.Close();

                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                
                RootEntity loadedEntity = LoadRootEntityWithId(connection,id);
                
                transaction.Commit();
                connection.Close();

                AssertTwoRootEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (ErManagementPatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }


        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_patchDataBase_withEmptyDb_shouldCreatePrimaryKeys_shouldNotAbleToPutDuplicateData()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            int id = 37;
            RootEntity entity = CreateRootEntityWithoutNullValues(id);
            entity.LeafEntities.Add(CreateLeafEntityA(id,1));
            entity.LeafEntities.Add(CreateLeafEntityB(id,1));
            entity.Persist(connection);

            transaction.Commit();
            connection.Close();

            connection = DbConnector.GetSharedInstance().Connection;
            connection.Open();
            RootEntity loadedEntity = LoadRootEntityWithId(connection,id);
            connection.Close();

            AssertTwoRootEntitiesEquals(entity,loadedEntity);
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_patchDataBase_withEmptyDb_shouldCreateForeignKeys_shouldNotAbleToInconsistantData()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            int id = 38;
            RootEntity entity = CreateRootEntityWithoutNullValues(id);
            entity.LeafEntities.Add(CreateLeafEntityA(id + 1,1));
            entity.LeafEntities.Add(CreateLeafEntityB(id,1));
            entity.Persist(connection);

            transaction.Commit();
            connection.Close();

            connection = DbConnector.GetSharedInstance().Connection;
            connection.Open();
            RootEntity loadedEntity = LoadRootEntityWithId(connection,id);
            connection.Close();

            AssertTwoRootEntitiesEquals(entity,loadedEntity);
        }

        [Test]
        public void ERLayer_patchDataBase_pathTwice_shouldNotThrowException()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;

            ICollection<IServerDbClass> dbClasses = new List<IServerDbClass>();
            dbClasses.Add(new LeafEntitySubA());
            dbClasses.Add(new LeafEntitySubB());
            dbClasses.Add(new RootEntity());

            ErLayer.GetSharedInstance().PatchDataBase(connection,dbClasses,true);
        }

        private void AssertTwoRootEntitiesEquals(RootEntity entityA, RootEntity entityB)
        {
            Assert.AreEqual(entityA.IdCol,entityB.IdCol);

            Assert.AreEqual(entityA.CharNotNull,entityB.CharNotNull);
            Assert.AreEqual(entityA.CharNull,entityB.CharNull);
            Assert.AreEqual(entityA.DateNotNull,entityB.DateNotNull);
            Assert.AreEqual(entityA.DateNull,entityB.DateNull);
            Assert.AreEqual(entityA.DoubleNotNull,entityB.DoubleNotNull);
            Assert.AreEqual(entityA.DoubleNull,entityB.DoubleNull);
            Assert.AreEqual(entityA.FloatNotNull,entityB.FloatNotNull);
            Assert.AreEqual(entityA.FloatNull,entityB.FloatNull);
            Assert.AreEqual(entityA.IntNotNull,entityB.IntNotNull);
            Assert.AreEqual(entityA.IntNull,entityB.IntNull);
            Assert.AreEqual(entityA.LongNotNull,entityB.LongNotNull);
            Assert.AreEqual(entityA.LongNull,entityB.LongNull);
            Assert.AreEqual(entityA.TimestampNotNull,entityB.TimestampNotNull);
            Assert.AreEqual(entityA.TimestampNull,entityB.TimestampNull);
            Assert.AreEqual(entityA.VarcharNotNull,entityB.VarcharNotNull);
            Assert.AreEqual(entityA.VarcharNull,entityB.VarcharNull);
            Assert.AreEqual(entityA.LeafEntities.Count,entityB.LeafEntities.Count);

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
            Assert.AreEqual(entityA.IdCol,entityB.IdCol);
            Assert.AreEqual(entityA.IndexNo,entityB.IndexNo);
            Assert.AreEqual(entityA.SomeText,entityB.SomeText);
            if (entityA is LeafEntitySubA && entityB is LeafEntitySubA)
            {
                Assert.AreEqual(((LeafEntitySubA)entityA).SomeTextA,((LeafEntitySubA)entityB).SomeTextA);
            }
            if (entityA is LeafEntitySubB && entityB is LeafEntitySubB)
            {
                Assert.AreEqual(((LeafEntitySubB)entityA).SomeTextB,((LeafEntitySubB)entityB).SomeTextB);
            }
        }

        private RootEntity LoadRootEntityWithId(IDbConnection connection,int id) 
        {
            RootEntity loadedEntity = null;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from root_entity where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new RootEntity();
                loadedEntity.Retrieve(rs,connection);
            }

            return loadedEntity;
        }

        private LeafEntity CreateLeafEntityA(int id,int index)
        {
            LeafEntitySubA leafEntity = new LeafEntitySubA();
            leafEntity.IdCol = id;
            leafEntity.IndexNo = index;
            leafEntity.SomeTextA = "text A";
            leafEntity.SomeText = "Id : " + id + " - " + " Index : " + index;

            return leafEntity;
        }

        private LeafEntity CreateLeafEntityB(int id,int index)
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
            entity.CharNull ='B';
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