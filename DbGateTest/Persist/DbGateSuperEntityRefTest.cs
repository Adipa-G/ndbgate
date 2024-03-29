using System;
using System.Data;
using DbGate.Persist.Support.SuperEntityRefInheritance;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateSuperEntityRefTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-super_entity_ref_test";

        public DbGateSuperEntityRefTest()
        {
            TestClass = typeof(DbGateSuperEntityRefTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
        
        private IDbConnection SetupTables()
        {
            var sql = "Create table super_entity_ref_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);

            sql = "Create table super_entity_ref_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table super_entity_ref_test_one2many_a (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname_a Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table super_entity_ref_test_one2many_b (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname_b Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table super_entity_ref_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table super_entity_ref_test_one2one_a (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname_a Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table super_entity_ref_test_one2one_b (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname_b Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            EndInit(DbName);
            return Connection;
        }

        [Fact]
        public void SuperEntityRef_PersistAndLoadWithSingleTypeA_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);
                
                var id = 35;
                var entity = CreateDefaultRootEntity(id, 1, 0, true, false);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(transaction,entityReloaded,id);
                con.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void SuperEntityRef_PersistAndLoadWithSingleTypeB_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var entity = CreateDefaultRootEntity(id, 0, 1, true, false);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                transaction.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void SuperEntityRef_PersistAndLoadWithAllTypeA_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var entity = CreateDefaultRootEntity(id, 10, 0, true, false);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                transaction.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void SuperEntityRef_PersistAndLoadWithAllTypeB_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var entity = CreateDefaultRootEntity(id, 0, 10, true, false);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                transaction.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void SuperEntityRef_PersistAndLoadWithMixedTypes_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var entity = CreateDefaultRootEntity(id, 10, 10, true, false);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                transaction.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void SuperEntityRef_PersistAndLoadWithNullOneToOne_RetrievedShouldBeSameAsPersisted()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);
                
                var id = 35;
                var entity = CreateDefaultRootEntity(id, 0, 0, false, false);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new SuperEntityRefRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                transaction.Close();

                VerifyEquals(entity, entityReloaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private void VerifyEquals(SuperEntityRefRootEntity rootEntity, SuperEntityRefRootEntity loadedRootEntity)
        {
            Assert.Equal(loadedRootEntity.IdCol,rootEntity.IdCol);
            Assert.Equal(loadedRootEntity.Name,rootEntity.Name);

            foreach (var one2ManyEntity in rootEntity.One2ManyEntities)
            {
                var foundItem = false;
                foreach (var loadedOne2ManyEntity in loadedRootEntity.One2ManyEntities)
                {
                    if (one2ManyEntity.IndexNo == loadedOne2ManyEntity.IndexNo)
                    {
                        foundItem = true;
                        Assert.Equal(one2ManyEntity.GetType(),loadedOne2ManyEntity.GetType());
                        Assert.Equal(one2ManyEntity.Name, loadedOne2ManyEntity.Name);

                        if (one2ManyEntity.GetType() == typeof(SuperEntityRefOne2ManyEntityA))
                        {
                            var one2ManyEntityA = (SuperEntityRefOne2ManyEntityA) one2ManyEntity;
                            var loadedOne2ManyEntityA = (SuperEntityRefOne2ManyEntityA) loadedOne2ManyEntity;
                            Assert.Equal(one2ManyEntityA.NameA,loadedOne2ManyEntityA.NameA);
                        }
                        else if (one2ManyEntity.GetType() == typeof(SuperEntityRefOne2ManyEntityB))
                        {
                            var one2ManyEntityA = (SuperEntityRefOne2ManyEntityB) one2ManyEntity;
                            var loadedOne2ManyEntityA = (SuperEntityRefOne2ManyEntityB) loadedOne2ManyEntity;
                            Assert.Equal(one2ManyEntityA.NameB,loadedOne2ManyEntityA.NameB);
                        }
                    }
                }
                Assert.True(foundItem,"Item rootEntity not found");
            }

            var one2OneEntity = rootEntity.One2OneEntity;
            var loadedOne2OneEntity = loadedRootEntity.One2OneEntity;
            if (one2OneEntity == null || loadedOne2OneEntity == null)
            {
                Assert.True(one2OneEntity == loadedOne2OneEntity,"One entity is null while other is not");
            }
            else
            {
                Assert.Equal(one2OneEntity.GetType(),loadedOne2OneEntity.GetType());
                Assert.Equal(one2OneEntity.Name,loadedOne2OneEntity.Name);

                loadedOne2OneEntity = loadedRootEntity.One2OneEntity; //in case of lazy loading
                if (one2OneEntity.GetType() == typeof(SuperEntityRefOne2OneEntityA))
                {
                    var one2OneEntityA = (SuperEntityRefOne2OneEntityA) one2OneEntity;
                    var loadedOne2OneEntityA = (SuperEntityRefOne2OneEntityA) loadedOne2OneEntity;
                    Assert.Equal(one2OneEntityA.NameA,loadedOne2OneEntityA.NameA);
                }
                else if (one2OneEntity.GetType() == typeof(SuperEntityRefOne2OneEntityB))
                {
                    var one2OneEntityB = (SuperEntityRefOne2OneEntityB) one2OneEntity;
                    var loadedOne2OneEntityB = (SuperEntityRefOne2OneEntityB) loadedOne2OneEntity;
                    Assert.Equal(one2OneEntityB.NameB,loadedOne2OneEntityB.NameB);
                }
            }
        }

        private SuperEntityRefRootEntity CreateDefaultRootEntity(int id,int typeACount,int typeBCount,bool one2OneIsA,bool one2OneIsB)
        {
            var entityText = string.Format("Id->{0}|A->{1}|B->{2}|OOA->{3}|OOB->{4}",id,typeACount,typeBCount,one2OneIsA,one2OneIsB);

            var rootEntity = new SuperEntityRefRootEntity();
            rootEntity.IdCol = id;
            rootEntity.Name = "Root-(" + entityText + ")";

            for (var i = 0; i < typeACount; i++)
            {
                rootEntity.One2ManyEntities.Add(CreateOne2Many(true,entityText,i));
            }
            for (var i = typeACount; i < typeACount + typeBCount; i++)
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
	        var entity = typeA
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
	        var entity = typeA
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

        private bool LoadEntityWithId(ITransaction transaction, SuperEntityRefRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from super_entity_ref_test_root where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            var dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, transaction);
                loaded = true;
            }

            return loaded;
        }
    }
}
