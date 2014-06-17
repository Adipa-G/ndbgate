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
using System.Linq;

namespace dbgate.ermanagement
{
    public class ErManagementQueryBasicTest
    {
        private int[] _basicEntityIds;
        private string[] _basicEntityNames;
        private bool[] _hasOverrideChildren;

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
                DbGate.GetSharedInstance().ClearCache();
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

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM query_basic_join";
                command.ExecuteNonQuery();
                
                command = connection.CreateCommand();
                command.CommandText = "drop table query_basic_join";
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

            string sql = "Create table query_basic (\n" +
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

            sql = "Create table query_basic_join (\n" +
                            "\tid_col Int NOT NULL,\n" +
						 	"\tname Varchar(20) NOT NULL,\n" +
						 	"\toverride_description Varchar(50) NOT NULL )";
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

        private bool HasIds(ICollection<object> list,params int[] idList)
        {
            if (list.Count != idList.Length)
                return false;

            foreach (int id in idList)
            {
                bool found = false;
                foreach (object listItem in list)
                {
                    if (listItem is QueryBasicEntity)
                    {
                        found = found || ((QueryBasicEntity)listItem).IdCol == id;
                    }
                    else if (listItem is object[])
                    {
                        object[] items = (object[])listItem;
                        foreach (object item in items)
                        {
                            if (item is int)
                            {
                                found = found || ((int)item) == id;
                            }
                        }
                    }
                    else
                    {
                        found = found || ((int)listItem) == id;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

		private void CreateTestData(IDbConnection connection)
	 	{
            _basicEntityIds = new[] { 35, 45, 55, 65 };
            _basicEntityNames = new[] { "Org-NameA", "Org-NameA", "Org-NameA", "Org-NameB" };
            _hasOverrideChildren = new[] { true, false, false, true };
            
            _basicEntities = new List<QueryBasicEntity>();
			_detailedEntities = new List<QueryBasicDetailsEntity>();
			
            for (int i = 0, basicEntityIdsLength = _basicEntityIds.Length; i < basicEntityIdsLength; i++)
            {
                int basicEntityId = _basicEntityIds[i];
                var entity = new QueryBasicEntity();
                entity.IdCol = basicEntityId;
                entity.Name = _basicEntityNames[i];
                if (_hasOverrideChildren[i])
                {
                    var joinEntity = new QueryBasicJoinEntity();
                    joinEntity.OverrideDescription = entity.Name + "Details";
                    entity.JoinEntity = joinEntity;
                }
                entity.Persist(connection);
                _basicEntities.Add(entity);
            }

            foreach (string basicEntityName in _basicEntityNames)
            {
                bool found = false;
                foreach (QueryBasicDetailsEntity entity in _detailedEntities)
                {
                    if (entity.Name.Equals(basicEntityName))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    continue;
                }
                var detailsEntity = new QueryBasicDetailsEntity();
                detailsEntity.Name = basicEntityName;
                detailsEntity.Description = basicEntityName + "Details";
                detailsEntity.Persist(connection);
                _detailedEntities.Add(detailsEntity);
            }
	 	}
    
        [Test]
        public void QueryBasic_Basic_WithSql_ShouldSelectAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Distinct_WithSql_ShouldSelectDistinct()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Skip_WithSql_ShouldSkip()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)))
					.Skip(1);

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds.Skip(1).ToArray());
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void QueryBasic_Fetch_WithSql_ShouldFetch()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)))
					.Fetch(2);

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds.Take(2).ToArray());
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void QueryBasic_SkipAndFetch_WithSql_ShouldSkipAndFetch()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)))
					.Skip(1).Fetch(2);

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds.Skip(1).Take(2).ToArray());
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

		[Test]
        public void QueryBasic_Select_WithTypeSelection_ShouldSelectAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Select_WithSubQuerySelection_ShouldSelectAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Select_WithFieldSelectionWithClass_ShouldSelectColumn()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();
                
                ISelectionQuery query = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                	.Select(QuerySelection.Field(typeof(QueryBasicEntity),"Name","name1"));
                
                ICollection<object> results = query.ToList(connection);
                Assert.IsTrue(results.Count == 4);
                int index = 0;
                foreach (object result in results)
                {
                    object[] resultArray = (object[]) result;
                    String name = (String) resultArray[0];
    
                    Assert.IsTrue(_basicEntityNames[index++].Equals(name));
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
        public void QueryBasic_Select_WithSumSelection_ShouldSelectSum()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Select_WithCountSelection_ShouldSelectCount()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Select_WithCustomFunctionCountAsExampleSelection_ShouldSelectCount()
		{
        	try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
         public void QueryBasic_From_WithTypeFrom_ShouldSelectAll()
         {
             try
             {
                 IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();
                 CreateTestData(connection);
                 transaction.Commit();

                 ISelectionQuery selectionQuery = new SelectionQuery()
                     .From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb1"))
                     .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                 ICollection<object> results = selectionQuery.ToList(connection);
                 HasIds(results, _basicEntityIds);
                 connection.Close();
             }
             catch (Exception e)
             {
                 LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                 Assert.Fail(e.Message);
             }
         }
 
        [Test]
         public void QueryBasic_From_WithQueryFrom_ShouldSelectAll()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();
                
                ISelectionQuery fromQuery = new SelectionQuery()
                	.From(QueryFrom.EntityType(typeof(QueryBasicEntity)));
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.Query(fromQuery,"qb1"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void QueryBasic_From_WithQueryUnionFrom_ShouldSelectUnion()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
                int index = 0;
                foreach (object result in results)
                {
                    var resultArray = (object[]) result;
                    var name = (string) resultArray[0];
    
                    if (index < 4)
                    {
                        Assert.IsTrue(_basicEntityNames[index++].Equals(name));
                    }
                    else
                    {
                        var detailsEntity = _detailedEntities.ToArray()[index++ - 4];
                        Assert.IsTrue(detailsEntity.Name.Equals(name));
                    }
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
        public void QueryBasic_Condition_WithSql_ShouldSelectMatching()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();
                
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
		 			.Where(QueryCondition.RawSql("id_col = 35"))
		 			.Where(QueryCondition.RawSql("name like 'Org-NameA'"))
		 			.Select(QuerySelection.RawSql("id_col,name"));

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, 35);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
		}

        [Test]
        public void QueryBasic_Condition_WithEqExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(ColumnType.Integer, 35)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithNEqExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Neq().Value(ColumnType.Integer,35)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds.Where(id => id != 35).ToArray());
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithGtExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Gt().Value(ColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds.Where(id => id > 45).ToArray());
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithGeExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Ge().Value(ColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds.Where(id => id >= 45).ToArray());
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithLtExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Lt().Value(ColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds.Where(id => id < 45).ToArray());
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithLeExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").Le().Value(ColumnType.Integer, 45)))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds.Where(id => id <= 45).ToArray());
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithLikeExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                        .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                        .Where(QueryCondition.Expression(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "Name").Like().Value(ColumnType.Varchar, "Org-NameA")))
                        .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35, 45, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithNeqExpression_WithField_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
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
        public void QueryBasic_Condition_WithGtExpression_WithQuery_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qbd1"))
                    .OrderBy(QueryOrderBy.RawSql("id_col"))
                    .Select(QuerySelection.Field(typeof(QueryBasicEntity), "IdCol", "id_col")).Fetch(1);

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").Gt().Query(subQuery)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds.Skip(1).ToArray());
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithBetweenExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").Between().Values(ColumnType.Integer, new object[]{35, 55})))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35, 45, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithInExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer, new object[] { 35, 55 })))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithInExpression_WithQuery_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Select(QuerySelection.Field(typeof(QueryBasicEntity), "IdCol", null));

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").In().Query(subQuery)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, _basicEntityIds);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithExistsExpression_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
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
                HasIds(results, _basicEntityIds);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithNotExistsExpression_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
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
        public void QueryBasic_Condition_WithSimpleMergeExpressionSingleAnd_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer,new object[]{35,55})
                        .And().Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer,new object[]{45,55})))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithSimpleMergeExpressionTwoOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer,new object[]{35,55})
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").In().Value(ColumnType.Integer, 55)
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer, new object[] { 45, 55 })))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35, 45, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithSimpleMergeExpressionSingleAndSingleOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer, new object[] { 35, 55 })
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").In().Value(ColumnType.Integer, 55)
                        .And().Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer, new object[] { 45, 55 })))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithComplexMergeExpressionCombinedTwoAndsWithOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .And(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer, new object[]{35, 55})
                                ,ConditionExpr.Build()
                                .Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(ColumnType.Integer, 55))
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 45, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithComplexMergeExpressionCombinedTwoOrsWithOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity)))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                       .Or(ConditionExpr.Build()
                            .Field(typeof(QueryBasicEntity), "IdCol").In().Values(ColumnType.Integer, new object[]{35, 55})
                                ,ConditionExpr.Build()
                                .Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(ColumnType.Integer, 55))
                        .Or().Field(typeof(QueryBasicEntity), "IdCol").Eq().Value(ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = query.ToList(connection);
                HasIds(results, 35, 45, 55);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithBasicSql_ShouldJoin()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Join(QueryJoin.RawSql("inner join query_basic_details qbd1 on qb1.name = qbd1.name"))
                    .OrderBy(QueryOrderBy.RawSql("qb1.name"))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicEntity)));

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithEntityDefinedJoinDirectionOfDefinition_ShouldJoin()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                    .Join(QueryJoin.EntityType(typeof(QueryBasicEntity),typeof(QueryBasicJoinEntity),"qbj1"))
                    .Select(QuerySelection.Field(typeof(QueryBasicEntity), "IdCol", null));

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, 35, 65);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithEntityDefinedJoinDirectionOppositeToDefinition_ShouldJoin()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicJoinEntity), "qbj1"))
                    .Join(QueryJoin.EntityType(typeof(QueryBasicJoinEntity), typeof(QueryBasicEntity), "qb1"))
                    .Select(QuerySelection.Field(typeof(QueryBasicJoinEntity), "IdCol", null));

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, 35, 65);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithEntityDefinedJoinWithOuterJoin_ShouldJoin()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicJoinEntity), "qbj1"))
                    .Join(QueryJoin.EntityType(typeof(QueryBasicJoinEntity), typeof(QueryBasicEntity), "qb1",QueryJoinType.Left))
                    .Select(QuerySelection.Field(typeof(QueryBasicJoinEntity), "IdCol", null));

                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithoutEntityDefinedJoinWithExpression_ShouldJoin()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                    .Join(QueryJoin.EntityType(typeof(QueryBasicEntity)
                            , typeof(QueryBasicDetailsEntity)
                            , JoinExpr.Build()
                                .Field(typeof(QueryBasicEntity),"Name").Eq().Field(typeof(QueryBasicDetailsEntity),"Name")
                            , "qbd1"))
                    .Select(QuerySelection.Field(typeof(QueryBasicDetailsEntity), "Description", null));


                ICollection<object> results = selectionQuery.ToList(connection);
                HasIds(results, _basicEntityIds);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
       
		[Test]
        public void QueryBasic_Group_WithBasicSql_ShouldGroup()
		{
			try
            {
                IDbConnection connection = SetupTables();
				IDbTransaction transaction = connection.BeginTransaction();
				CreateTestData(connection);
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
        public void QueryBasic_Group_WithExpression_ShouldGroup()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb"))
                    .GroupBy(QueryGroup.Field(typeof(QueryBasicEntity), "Name"))
                    .Select(QuerySelection.Field(typeof(QueryBasicEntity), "Name", null));

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
				CreateTestData(connection);
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
        public void QueryBasic_GroupCondition_WithExpressionCount_ShouldSelectMatchingGroups()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity), "qb"))
                    .Select(QuerySelection.Field(typeof(QueryBasicEntity), "Name", null))
                    .GroupBy(QueryGroup.Field(typeof(QueryBasicEntity), "Name"))
                    .Having(QueryGroupCondition.Expression(
                            GroupConditionExpr.Build()
                                .Field(typeof(QueryBasicEntity), "Name").Count().Gt().Value(ColumnType.Integer,1)
                    ));

                ICollection<object> results = query.ToList(connection);
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
				CreateTestData(connection);
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
        public void QueryBasic_OrderBy_WithExpression_ShouldSelectOrdered()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .OrderBy(QueryOrderBy.Field(typeof(QueryBasicEntity),"Name"))
                    .OrderBy(QueryOrderBy.Field(typeof(QueryBasicEntity),"IdCol"))
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

        public void QueryBasic_OrderBy_WithExpressionDesc_ShouldSelectOrdered()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                CreateTestData(connection);
                transaction.Commit();

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .OrderBy(QueryOrderBy.Field(typeof(QueryBasicEntity), "Name",QueryOrderType.Descend))
                    .OrderBy(QueryOrderBy.Field(typeof(QueryBasicEntity), "IdCol",QueryOrderType.Descend))
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
    }
}
