using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Castle.DynamicProxy;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using DbGate.Support.Persistant.SuperEntityRefInheritance;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateSuperEntityRefTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateSuperEntityRefTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
				Assert.IsNotNull(dbConnector.Connection);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateSuperEntityRefTest)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (DbGateSuperEntityRefTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().ClearCache();
            }
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_one2many_a";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_one2many_a";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_one2many_b";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_one2many_b";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_one2one_a";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_one2one_a";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM super_entity_ref_test_one2one_b";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table super_entity_ref_test_one2one_b";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            string sql = "Create table super_entity_ref_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(100) NOT NULL,\n" +
                             " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table super_entity_ref_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table super_entity_ref_test_one2many_a (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname_a Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table super_entity_ref_test_one2many_b (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname_b Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table super_entity_ref_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table super_entity_ref_test_one2one_a (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname_a Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table super_entity_ref_test_one2one_b (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname_b Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }
    
        [Test]
        public void SuperEntityRef_PersistAndLoadWithSingleTypeA_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                SuperEntityRefRootEntity entity = CreateDefaultRootEntity(id, 1, 0, true, false);
                entity.Persist(connection);
                transaction.Commit();

                SuperEntityRefRootEntity entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(connection,entityReloaded,id);
                connection.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void SuperEntityRef_PersistAndLoadWithSingleTypeB_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                SuperEntityRefRootEntity entity = CreateDefaultRootEntity(id, 0, 1, true, false);
                entity.Persist(connection);
                transaction.Commit();

                SuperEntityRefRootEntity entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                connection.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void SuperEntityRef_PersistAndLoadWithAllTypeA_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                SuperEntityRefRootEntity entity = CreateDefaultRootEntity(id, 10, 0, true, false);
                entity.Persist(connection);
                transaction.Commit();

                SuperEntityRefRootEntity entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                connection.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void SuperEntityRef_PersistAndLoadWithAllTypeB_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                SuperEntityRefRootEntity entity = CreateDefaultRootEntity(id, 0, 10, true, false);
                entity.Persist(connection);
                transaction.Commit();

                SuperEntityRefRootEntity entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                connection.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void SuperEntityRef_PersistAndLoadWithMixedTypes_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                SuperEntityRefRootEntity entity = CreateDefaultRootEntity(id, 10, 10, true, false);
                entity.Persist(connection);
                transaction.Commit();

                SuperEntityRefRootEntity entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                connection.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void SuperEntityRef_PersistAndLoadWithNullOneToOne_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                SuperEntityRefRootEntity entity = CreateDefaultRootEntity(id, 0, 0, false, false);
                entity.Persist(connection);
                transaction.Commit();

                SuperEntityRefRootEntity entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                connection.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private void VerifyEquals(SuperEntityRefRootEntity rootEntity, SuperEntityRefRootEntity loadedRootEntity)
        {
            Assert.AreEqual(loadedRootEntity.IdCol,rootEntity.IdCol);
            Assert.AreEqual(loadedRootEntity.Name,rootEntity.Name);

            foreach (SuperEntityRefOne2ManyEntity one2ManyEntity in rootEntity.One2ManyEntities)
            {
                bool foundItem = false;
                foreach (SuperEntityRefOne2ManyEntity loadedOne2ManyEntity in loadedRootEntity.One2ManyEntities)
                {
                    if (one2ManyEntity.IndexNo == loadedOne2ManyEntity.IndexNo)
                    {
                        foundItem = true;
                        Assert.AreEqual(one2ManyEntity.GetType(),loadedOne2ManyEntity.GetType());
                        Assert.AreEqual(one2ManyEntity.Name, loadedOne2ManyEntity.Name);

                        if (one2ManyEntity.GetType() == typeof(SuperEntityRefOne2ManyEntityA))
                        {
                            SuperEntityRefOne2ManyEntityA one2ManyEntityA = (SuperEntityRefOne2ManyEntityA) one2ManyEntity;
                            SuperEntityRefOne2ManyEntityA loadedOne2ManyEntityA = (SuperEntityRefOne2ManyEntityA) loadedOne2ManyEntity;
                            Assert.AreEqual(one2ManyEntityA.NameA,loadedOne2ManyEntityA.NameA);
                        }
                        else if (one2ManyEntity.GetType() == typeof(SuperEntityRefOne2ManyEntityB))
                        {
                            SuperEntityRefOne2ManyEntityB one2ManyEntityA = (SuperEntityRefOne2ManyEntityB) one2ManyEntity;
                            SuperEntityRefOne2ManyEntityB loadedOne2ManyEntityA = (SuperEntityRefOne2ManyEntityB) loadedOne2ManyEntity;
                            Assert.AreEqual(one2ManyEntityA.NameB,loadedOne2ManyEntityA.NameB);
                        }
                    }
                }
                Assert.IsTrue(foundItem,"Item rootEntity not found");
            }

            SuperEntityRefOne2OneEntity one2OneEntity = rootEntity.One2OneEntity;
            SuperEntityRefOne2OneEntity loadedOne2OneEntity = loadedRootEntity.One2OneEntity;
            if (one2OneEntity == null || loadedOne2OneEntity == null)
            {
                Assert.IsTrue(one2OneEntity == loadedOne2OneEntity,"One entity is null while other is not");
            }
            else
            {
                Assert.AreEqual(one2OneEntity.GetType(),loadedOne2OneEntity.GetType());
                Assert.AreEqual(one2OneEntity.Name,loadedOne2OneEntity.Name);
                if (one2OneEntity.GetType() == typeof(SuperEntityRefOne2OneEntityA))
                {
                    SuperEntityRefOne2OneEntityA one2OneEntityA = (SuperEntityRefOne2OneEntityA) one2OneEntity;
                    SuperEntityRefOne2OneEntityA loadedOne2OneEntityA = (SuperEntityRefOne2OneEntityA) loadedOne2OneEntity;
                    Assert.AreEqual(one2OneEntityA.NameA,loadedOne2OneEntityA.NameA);
                }
                else if (one2OneEntity.GetType() == typeof(SuperEntityRefOne2OneEntityB))
                {
                    SuperEntityRefOne2OneEntityB one2OneEntityB = (SuperEntityRefOne2OneEntityB) one2OneEntity;
                    SuperEntityRefOne2OneEntityB loadedOne2OneEntityB = (SuperEntityRefOne2OneEntityB) loadedOne2OneEntity;
                    Assert.AreEqual(one2OneEntityB.NameB,loadedOne2OneEntityB.NameB);
                }
            }
        }

        private SuperEntityRefRootEntity CreateDefaultRootEntity(int id,int typeACount,int typeBCount,bool one2OneIsA,bool one2OneIsB)
        {
            string entityText = string.Format("Id->{0}|A->{1}|B->{2}|OOA->{3}|OOB->{4}",id,typeACount,typeBCount,one2OneIsA,one2OneIsB);

            SuperEntityRefRootEntity rootEntity = new SuperEntityRefRootEntity();
            rootEntity.IdCol = id;
            rootEntity.Name = "Root-(" + entityText + ")";

            for (int i = 0; i < typeACount; i++)
            {
                rootEntity.One2ManyEntities.Add(CreateOne2Many(true,entityText,i));
            }
            for (int i = typeACount; i < typeACount + typeBCount; i++)
            {
                rootEntity.One2ManyEntities.Add(CreateOne2Many(false,entityText,i));
            }
            if (one2OneIsA)
            {
                rootEntity.One2OneEntity = CreateOne2One(true,entityText);
            }
            if (one2OneIsB)
            {
                rootEntity.One2OneEntity = CreateOne2One(false,entityText);
            }
            
            return rootEntity;
        }
	    
	    private SuperEntityRefOne2ManyEntity CreateOne2Many(bool typeA,String entityText,int index)
	    {
	        SuperEntityRefOne2ManyEntity entity = typeA
	                                                  ? new SuperEntityRefOne2ManyEntityA()
	                                                  : (SuperEntityRefOne2ManyEntity) new SuperEntityRefOne2ManyEntityB();

	        entity.IndexNo = index;
	        entity.Name = "OM-S-(" + entityText + ")" + index;
            if (entity.GetType() == typeof(SuperEntityRefOne2ManyEntityA))
            {
                ((SuperEntityRefOne2ManyEntityA)entity).NameA = "OM-A-(" + entityText + ")" + index;
            }
            else if (entity.GetType() == typeof(SuperEntityRefOne2ManyEntityB))
            {
                ((SuperEntityRefOne2ManyEntityB)entity).NameB = "OM-B-(" + entityText + ")" + index;
            }
            return entity;
        }

    		
	    private SuperEntityRefOne2OneEntity CreateOne2One(bool typeA,String entityText)
	    {
	        SuperEntityRefOne2OneEntity entity = typeA
	                                                 ? new SuperEntityRefOne2OneEntityA()
	                                                 : (SuperEntityRefOne2OneEntity) new SuperEntityRefOne2OneEntityB();
	
	        entity.Name = "OO-S-(" + entityText + ")";
	        if (entity.GetType() == typeof(SuperEntityRefOne2OneEntityA))
	        {
	            ((SuperEntityRefOne2OneEntityA)entity).NameA = "OO-A-(" + entityText + ")";
	        }
	        else if (entity.GetType() == typeof(SuperEntityRefOne2OneEntityB))
	        {
	            ((SuperEntityRefOne2OneEntityB)entity).NameB ="OO-B-(" + entityText + ")";
	        }
	        return entity;
        }

        private bool LoadEntityWithId(IDbConnection connection, SuperEntityRefRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from super_entity_ref_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, connection);
                loaded = true;
            }

            return loaded;
        }
    }
}
