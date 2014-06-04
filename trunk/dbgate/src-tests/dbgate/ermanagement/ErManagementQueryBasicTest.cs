using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.query;
using dbgate.ermanagement.query.expr;
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
    	private ICollection<QueryBasicEntity> _basicEntities;
    	private ICollection<QueryBasicDetailsEntity> _detailedEntities;
    	
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

				command = connection.CreateCommand();
                command.CommandText = "DELETE FROM query_basic_details";
                command.ExecuteNonQuery();
                
                command = connection.CreateCommand();
                command.CommandText = "drop table query_basic_details";
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


			sql = "Create table query_basic_details (\n" +
						 	"\tname Varchar(20) NOT NULL,\n" +
						 	"\tdescription Varchar(50) NOT NULL )";
			cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }
        
        private QueryBasicEntity GetById(int id)
	 	{
        	foreach (QueryBasicEntity basicEntity in _basicEntities)
		 	{
		 		if (basicEntity.IdCol == id)
		 			return basicEntity;
		 		}
		 		return null;
		 	}
		 	
		 	private QueryBasicDetailsEntity GetDescriptionForName(String name)
		 	{
			 	foreach (QueryBasicDetailsEntity detailsEntity in _detailedEntities)
			 	{
			 		if (detailsEntity.Name.Equals(name))
			 			return detailsEntity;
		 		}
			 	return null;
			}

		private void createTestData(IDbConnection connection)
	 	{
			_basicEntities = new List<QueryBasicEntity>();
			_detailedEntities = new List<QueryBasicDetailsEntity>();
			
		 	int id = 35;
		 	QueryBasicEntity entity = new QueryBasicEntity();
		 	entity.IdCol =id;
		 	entity.Name = "Org-NameA";
		 	entity.Persist(connection);
		 	_basicEntities.Add(entity);

			QueryBasicDetailsEntity detailsEntity = new QueryBasicDetailsEntity();
			detailsEntity.Name = entity.Name;
			detailsEntity.Description = entity.Name + "Details";
			detailsEntity.Persist(connection);
			_detailedEntities.Add(detailsEntity);
		 	
		 	id = 45;
		 	entity = new QueryBasicEntity();
		 	entity.IdCol = id;
		 	entity.Name = "Org-NameA";
		 	entity.Persist(connection);
		 	_basicEntities.Add(entity);

			id = 55;
		 	entity = new QueryBasicEntity();
		 	entity.IdCol = id;
		 	entity.Name = "Org-NameA";
		 	entity.Persist(connection);
		 	_basicEntities.Add(entity);
		 	
		 	id = 65;
		 	entity = new QueryBasicEntity();
		 	entity.IdCol = id;
			entity.Name = "Org-NameB";
		 	entity.Persist(connection);
		 	_basicEntities.Add(entity);

			detailsEntity = new QueryBasicDetailsEntity();
			detailsEntity.Name = entity.Name;
			detailsEntity.Description = entity.Name + "Details";
			detailsEntity.Persist(connection);
			_detailedEntities.Add(detailsEntity);
	 	}
    
        [Test]
        public void ERQuery_ExecuteToRetrieveAll_WithBasicSqlQuery_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("id_col"))
					.Select(QuerySelection.RawSql("name as name_col"));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
				{
					object[] resultArray = (object[]) result;
					int id = (int) resultArray[0];
					string name = (string) resultArray[1];
				
					QueryBasicEntity entity = GetById(id);
					Assert.AreEqual(entity.Name,name);
				}
              	connection.Close();
            }
            catch (Exception e)
            {
            	LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void ERQuery_ExecuteToRetrieveAll_WithDistinctSqlQuery_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("name as name_col"))
					.Distinct();

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 2);
                int count = 0;
				foreach (object result in results)
				{
					object[] resultArray = (object[]) result;
					string name = (string) resultArray[0];
					foreach (QueryBasicEntity basicEntity in _basicEntities)
					{
						if (basicEntity.Name.Equals(name))
							count++;
					}
				}
				Assert.IsTrue(count == 4);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void ERQuery_ExecuteToRetrieveAll_WithRowSkipSqlQuery_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("name as name_col"))
					.Skip(1);

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 3);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void ERQuery_ExecuteToRetrieveAll_WithFetchSqlQuery_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("name as name_col"))
					.Fetch(2);

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

		[Test]
        public void ERQuery_ExecuteToRetrieveAll_WithSkipAndFetchSqlQuery_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("name as name_col"))
					.Skip(1).Fetch(2);

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

		[Test]
        public void ERQuery_ExecuteToRetrieveAll_WithTypeSelection_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
				{
					object[] resultArray = (object[]) result;
					var loadedEntity = (QueryBasicEntity) resultArray[0];
					
					QueryBasicEntity orgEntity = GetById(loadedEntity.IdCol);
					Assert.AreEqual(loadedEntity.Name,orgEntity.Name);
				}
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void ERQuery_ExecuteToRetrieveAll_WithTypeFrom_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

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
        public void ERQuery_ExecuteToRetrieveAll_WithSubQuerySelection_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery descriptionQuery = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicDetailsEntity),"qbd1"))
		 			.Where(QueryCondition.RawSql("qbd1.name = qb1.name"))
		 			.Select(QuerySelection.RawSql("qbd1.description")).Fetch(1);
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)))
                    .Select(QuerySelection.Query(descriptionQuery, "description"));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
				{
					object[] resultArray = (object[]) result;
					var entity = (QueryBasicEntity) resultArray[0];
					string description = (string) resultArray[1];
				
					QueryBasicDetailsEntity detailsEntity = GetDescriptionForName(entity.Name);
					Assert.AreEqual(detailsEntity.Description,description);
				}
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void ERQuery_ExecuteToRetrieveAll_WithColumnSelection_shouldLoadAll()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery query = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                	.Select(QuerySelection.Column(typeof(QueryBasicEntity),"Name","name1"));
                
                ICollection<object> results = query.ToList(connection);
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
        public void ERQuery_ExecuteToRetrieveAll_WithSumSelection_shouldGetSum()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery query = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                	.Select(QuerySelection.Sum(typeof(QueryBasicEntity),"IdCol","id_sum"));
                
                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 1);
                int sum = 0;
				foreach (QueryBasicEntity entity in _basicEntities)
				{
					sum += entity.IdCol;
				}
				foreach (Object result in results)
				{
					Object[] resultArray = (Object[]) result;
					long resultSum = (long) resultArray[0];
					Assert.IsTrue(sum == resultSum);
				}
				connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void ERQuery_ExecuteToRetrieveAll_WithCountSelection_shouldGetCount()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery query = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                	.Select(QuerySelection.Count(typeof(QueryBasicEntity),"IdCol","id_count"));
                
                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 1);
               	foreach (Object result in results)
				{
					Object[] resultArray = (Object[]) result;
					long resultCount = (long) resultArray[0];
					Assert.IsTrue(resultCount == 4);
				}
				connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
        
         [Test]
        public void ERQuery_ExecuteToRetrieveAll_WithCustomFunctionSelectionWithCount_shouldGetCount()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery query = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                	.Select(QuerySelection.CustFunction("COUNT",typeof(QueryBasicEntity),"IdCol","id_count"));
                
                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 1);
               	foreach (Object result in results)
				{
					Object[] resultArray = (Object[]) result;
					long resultCount = (long) resultArray[0];
					Assert.IsTrue(resultCount == 4);
				}
				connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
 
        [Test]
        public void ERQuery_ExecuteToRetrieveAll_WithQueryFrom_shouldLoadAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery fromQuery = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity)));
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.Query(fromQuery,"qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

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
        public void ERQuery_ExecuteToRetrieveAll_WithQueryUnionFrom_shouldLoadUnion()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery fromBasic = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                	.Select(QuerySelection.RawSql("name as name1"));
                
                ISelectionQuery fromDetails = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicDetailsEntity)))
                	.Select(QuerySelection.RawSql("name as name1"));
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                	.From(QueryFrom.QueryUnion(true,new []{fromBasic,fromDetails}))
                    .Select(QuerySelection.RawSql("name1"));

                ICollection<object> results = selectionQuery.ToList(connection);
                Assert.IsTrue(results.Count == 6);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
       
		[Test]
		public void ERQuery_ExecuteWithCondition_WithBasicSqlQuery_shouldLoadTarget()
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
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderNEq_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Neq().Value(DbColumnType.Integer,35)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderGt_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Gt().Value(DbColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderGe_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Ge().Value(DbColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderLt_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Lt().Value(DbColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 1);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderLe_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Le().Value(DbColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderLike_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "Name").Like().Value(DbColumnType.Varchar, "Org-NameA")))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderNEqWithField_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicDetailsEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "Name").Neq().Field(typeof(QueryBasicDetailsEntity), "Description")))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicDetailsEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderGtWithQuery_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qbd1"))
                    .OrderBy(QueryOrderBy.RawSql("id_col"))
                    .Select(QuerySelection.Column(typeof(QueryBasicEntity), "IdCol", "id_col")).Fetch(1);

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").Gt().Query(subQuery)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderBetween_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").Between().Values(DbColumnType.Integer, new object[]{35, 55})))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderInWithValues_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer, new object[] { 35, 55 })))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderInWithQuery_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Select(QuerySelection.Column(typeof(QueryBasicEntity), "IdCol", null));

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").In().Query(subQuery)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 4);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderExists_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicDetailsEntity),"qbd1"))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicDetailsEntity),"qbd1","Name").Eq().Field(typeof(QueryBasicEntity),"qb1","Name")))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicDetailsEntity)));

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Query(subQuery).Exists()))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 4);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderNotExists_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicDetailsEntity), "qbd1"))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicDetailsEntity), "qbd1", "Name").Eq().Field(typeof(QueryBasicEntity), "qb1", "Name")))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicDetailsEntity)));

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Query(subQuery).NotExists()))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 0);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderAnd_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer,new object[]{35,55})
                        .And().Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer,new object[]{45,55})))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 1);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderOr_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer,new object[]{35,55})
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").In().Value(DbColumnType.Integer, 55)
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer, new object[] { 45, 55 })))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderOrAnd_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer, new object[] { 35, 55 })
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").In().Value(DbColumnType.Integer, 55)
                        .And().Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer, new object[] { 45, 55 })))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderMergeCombinationAnd_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .And(new[]{ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer, new object[]{35, 55})
                                ,ConditionExpr.Build()
                                .Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(DbColumnType.Integer, 55)})
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(DbColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERQuery_ExecuteWithCondition_WithExpressionBuilderMergeCombinationOr_shouldLoadTarget()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                createTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .Or(new[]{ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").In().Values(DbColumnType.Integer, new object[]{35, 55})
                                ,ConditionExpr.Build()
                                .Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(DbColumnType.Integer, 55)})
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(DbColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 3);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
		public void ERQuery_ExecuteWithGroup_WithBasicSqlQuery_shouldLoadTarget ()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
		 			.GroupBy(QueryGroup.RawSql("Name"))
				 	.Select(QuerySelection.RawSql("Name"));

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

		[Test]
		public void ExecuteWithGroupCondition_WithBasicSqlQuery_shouldLoadTarget()
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
					.Having(QueryGroupCondition.RawSql("count(id_col)>1"))
				 	.Select(QuerySelection.RawSql("name"));

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
		public void ExecuteWithOrderBy_WithBasicSqlQuery_shouldLoadTarget()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
		 			.OrderBy(QueryOrderBy.RawSql("name"))
				 	.Select(QuerySelection.RawSql("name"));

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
		public void ExecuteWithJoin_WithBasicSqlQuery_shouldLoadTarget()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				createTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
				 	.Join(QueryJoin.RawSql("inner join query_basic_details qbd1 on qb1.name = qbd1.name"))
				 	.OrderBy(QueryOrderBy.RawSql("qb1.name"))
				 	.Select(QuerySelection.RawSql("qb1.name as name"))
				 	.Select(QuerySelection.RawSql("description"));

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
    }
}
