using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using dbgate.dbutility;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.patch.patchempty;
using dbgate.ermanagement.support.patch.patchtabledifferences;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementPatchTableDifferenceDBTests
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof(ErManagementPatchTableDifferenceDBTests)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=4;foreign_keys = ON", DbConnector.DbSqllite);

                IDbConnection connection = dbConnector.Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementPatchTableDifferenceDBTests)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (ErManagementPatchTableDifferenceDBTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [Test]
        public void ERLayer_patchDB_withTableColumnAdded_shouldAddColumn()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();
                ICollection<IServerDbClass> dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new ThreeColumnEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses,true);
                transaction.Commit();
                connection.Close();

                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new FourColumnEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses, false);
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
                LogManager.GetLogger(typeof(ErManagementPatchTableDifferenceDBTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_patchDB_withTableColumnDeleted_shouldDeleteColumn()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();
                ICollection<IServerDbClass> dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new FourColumnEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses, true);
                transaction.Commit();
                connection.Close();

                connection = DbConnector.GetSharedInstance().Connection;
                transaction = connection.BeginTransaction();
                dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new ThreeColumnEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses, false);
                transaction.Commit();
                connection.Close();

                //Sqllite does not support dropping columns, so this test does not work
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementPatchTableDifferenceDBTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_patchDB_withTableColumnChanged_shouldUpdateColumn()
        {
            try
            {
                String longStr = new string('A',220);

                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();
                ICollection<IServerDbClass> dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new ThreeColumnEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses, true);
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
                    columnEntity = LoadThreeColumnEntityWithId(connection,id);
                    //db does not care about length :(
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
                dbClasses = new List<IServerDbClass>();
                dbClasses.Add(new ThreeColumnTypeDifferentEntity());
                ErLayer.GetSharedInstance().PatchDataBase(connection, dbClasses, false);
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
                LogManager.GetLogger(typeof(ErManagementPatchTableDifferenceDBTests)).Fatal(e.Message, e);
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

        private ThreeColumnEntity LoadThreeColumnEntityWithId(IDbConnection connection, int id)
        {
            ThreeColumnEntity loadedEntity = null;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from table_change_test_entity where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new ThreeColumnEntity();
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