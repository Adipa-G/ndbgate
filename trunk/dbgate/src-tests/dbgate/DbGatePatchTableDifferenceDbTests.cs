using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.support.patch.patchtabledifferences;
using log4net;
using NUnit.Framework;

namespace dbgate
{
    public class DbGatePatchTableDifferenceDbTests
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof(DbGatePatchTableDifferenceDbTests)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=4;foreign_keys = ON", DbConnector.DbSqllite);

                IDbConnection connection = dbConnector.Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGatePatchTableDifferenceDbTests)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnAdded_ShouldAddColumn()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();
                ICollection<Type> types = new List<Type>();
                types.Add(typeof(ThreeColumnEntity));
                DbGate.GetSharedInstance().PatchDataBase(connection, types,true);
                transaction.Commit();
                connection.Close();

                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                types = new List<Type>();
                types.Add(typeof(FourColumnEntity));
                DbGate.GetSharedInstance().PatchDataBase(connection, types, false);
                transaction.Commit();
                connection.Close();

                int id = 35;
                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                FourColumnEntity columnEntity = CreateFourColumnEntity(id);
                columnEntity.Persist(connection);
                columnEntity = LoadFourColumnEntityWithId(connection,id);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnDeleted_ShouldDeleteColumn()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();
                ICollection<Type> types = new List<Type>();
                types.Add(typeof(FourColumnEntity));
                DbGate.GetSharedInstance().PatchDataBase(connection, types, true);
                transaction.Commit();
                connection.Close();

                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                types = new List<Type>();
                types.Add(typeof(ThreeColumnEntity));
                DbGate.GetSharedInstance().PatchDataBase(connection, types, false);
                transaction.Commit();
                connection.Close();

                //Sqllite does not support dropping columns, so this test does not work
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnChanged_ShouldUpdateColumn()
        {
            try
            {
                string longStr = new string('A',220);

                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();
                ICollection<Type> types = new List<Type>();
                types.Add(typeof(ThreeColumnEntity));
                DbGate.GetSharedInstance().PatchDataBase(connection, types, true);
                transaction.Commit();
                connection.Close();

                int id = 34;
                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                ThreeColumnEntity columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                try
                {
                    columnEntity.Persist(connection);
                }
                catch (AssertionException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
                connection.Close();

                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                types = new List<Type>();
                types.Add(typeof(ThreeColumnTypeDifferentEntity));
                DbGate.GetSharedInstance().PatchDataBase(connection, types, false);
                transaction.Commit();
                connection.Close();

                id = 35;
                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                columnEntity.Persist(connection);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private FourColumnEntity LoadFourColumnEntityWithId(IDbConnection connection, int id)
        {
            FourColumnEntity loadedEntity = null;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from table_change_test_entity where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new FourColumnEntity();
                loadedEntity.Retrieve(rs, connection);
            }

            return loadedEntity;
        }

        private FourColumnEntity CreateFourColumnEntity(int id)
        {
            FourColumnEntity entity = new FourColumnEntity();
            entity.IdCol = id;
            entity.Code = "4C";
            entity.Name ="4Col";
            entity.IndexNo =0;
            return entity;
        }
    
        private ThreeColumnEntity CreateThreeColumnEntity(int id)
        {
            ThreeColumnEntity entity = new ThreeColumnEntity();
            entity.IdCol = id;
            entity.Name = "3Col";
            entity.IndexNo = 0;
            return entity;
        }   
    }
}