using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.query;
using dbgate.ermanagement.support.query.basic;
using log4net;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace dbgate.ermanagement
{
    public class ErManagementQueryBasicTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
				Assert.IsNotNull(dbConnector.Connection);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErLayer.GetSharedInstance().ClearCache();
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
                command.CommandText = "DELETE FROM query_basic";
                command.ExecuteNonQuery();
                
                command = connection.CreateCommand();
                command.CommandText = "drop table query_basic";
                command.ExecuteNonQuery();
                
                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            String sql = "Create table query_basic (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

		private void createTestData(IDbConnection connection)
	 	{
		 	int id = 35;
		 	QueryBasicEntity entity = new QueryBasicEntity();
		 	entity.IdCol =id;
		 	entity.Name = "Org-NameA";
		 	entity.Persist(connection);
		 	
		 	id = 45;
		 	entity = new QueryBasicEntity();
		 	entity.IdCol = id;
		 	entity.Name = "Org-NameA";
		 	entity.Persist(connection);
		 	
		 	id = 55;
		 	entity = new QueryBasicEntity();
		 	entity.IdCol = id;
		 	entity.Name = "Org-NameA";
		 	entity.Persist(connection);
		 	
		 	id = 65;
		 	entity = new QueryBasicEntity();
		 	entity.IdCol = id;
			entity.Name = "Org-NameB";
		 	entity.Persist(connection);
	 	}
    
        [Test]
        public void ERQuery_loadAll_WithBasicSqlQuery_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("id_col,name"));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 4);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
		public void ERQuery_loadWithCondition_WithBasicSqlQuery_shouldLoadTarget()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
		 			.Where(QueryCondition.RawSql("id_col = 35"))
		 			.Where(QueryCondition.RawSql("name like 'Org-NameA'"))
		 			.Select(QuerySelection.RawSql("id_col,name"));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 1);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
		}

		[Test]
		public void ERQuery_loadWithGroup_WithBasicSqlQuery_shouldLoadTarget ()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
		 			.GroupBy(QueryGroup.RawSql("name"))
				 	.Select(QuerySelection.RawSql("name"));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 2);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
		}
    }
}
