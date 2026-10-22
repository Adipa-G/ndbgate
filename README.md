# Introduction
NDbGate is a high performance ORM. The framework provides granular level control over persistence/retrieval.

### Features
* .Net Standard 2.1 (tested on .NET 10)
* Core relationships (One to One, One to Many, Inheritance)
* Change tracking
* Data verification on persisting
* Lazy loading
* Data migration
* Strong query language
* Parallel operation with multiple databases
* Easily used with legacy databases

### Using the library
Run time following dependencies are needed
* Castle.Core

### Quick Start
#### Define Entities
There are 3 ways to define entities
* Use attributes
* Extending an abstract class
* Registering entities manually

However for ease of understanding here only the annotations based approach would be used.All the entities has to be implemented the interface 
>IEntity.

The class
>DefaultEntity

is designed to used as super class for any entity.

   	[TableInfo("simple_entity")]
	public class SimpleEntity : DefaultEntity
	{
		[ColumnInfo(ColumnType.Integer, Key = true)]
		public int Id { get; set; }

		[ColumnInfo(ColumnType.Varchar)]
		public string Name { get; set; }
	}

Above class is a entity definition for a class with only 2 columns, first is named id and is a key, latter is a varchar column. Attribute
>TableInfo 

is used to define the name of the table the class supposed to be mapped. 
>ColumnInfo

is used to define the table column which the field is supposed to mapped.

#### Persisting
Persisting is straightforward

	SimpleEntity entity = new SimpleEntity();
	//set values to the entity
	entity.Persist(tx);  //tx is dbgate.ITransaction which created using dbgate.ITransactionFactory 

#### Retrieving
To retrieve an entity from the database without using dbgate queries, there must be a resultset pointing to the record to fetch.

	SimpleEntity entity = new SimpleEntity();
	//set values to the entity
	entity.Retrieve(rs,tx);  //use the java.sql.ResultSet to the record and tx is dbgate.ITransaction which created using dbgate.ITransactionFactory

#### Data Migration
Data migration pretty easy

	ICollection<Type> entityTypes = new ArrayList<Type>();
	entityTypes.Add(typeof(SimpleEntity));
	DbGate.GetSharedInstance().PatchDataBase(tx,entityTypes,false); //if the last parameter is true it would drop all the existing tables

#### Strong queries
Strong queries have support for many complex scenarios like sub queries, unions and group conditions. However for simplicity only a basic example is listed below

	ISelectionQuery query = new SelectionQuery()
					.From(QueryFrom.EntityType<SimpleEntity>())
					.Select(QuerySelection.EntityType<SimpleEntity>());
	ICollection entities = query.ToList(tx);

More examples can be found in the wiki. Also there is a sample project using the library available in the sources named DbGateTestApp.

### Performance
#### Test entities
	
	public abstract class Item
    {
        public int ItemId { get; set; }

        public string Name { get; set; }
    }
	
	public class Product : Item
    {
        public double UnitPrice { get; set; }

        public double? BulkUnitPrice { get; set; }
    }
	
	public class Service : Item
    {
        public double HourlyRate { get; set; }
    }
	
	public class Transaction
    {
        public int TransactionId { get; set; }

        public string Name { get; set; }

        public ICollection<ItemTransaction> ItemTransactions { get; set; }
    }
	
	public class ItemTransaction
    {
        public int TransactionId { get; set; }

        public int IndexNo { get; set; }

        public Item Item { get; set; }

        public Transaction Transaction { get; set; }

        public ICollection<ItemTransactionCharge> ItemTransactionCharges { get; set; }
    }
	
	public class ItemTransactionCharge
    {
        public int TransactionId { get; set; }

        public int IndexNo { get; set; }

        public int ChargeIndex { get; set; }

        public string ChargeCode { get; set; }

        public Transaction Transaction { get; set; }

        public ItemTransaction ItemTransaction { get; set; }
    }
	
#### Test

Inserting/ Querying/ Updating/ Deleting 100, 1000 and 5000 `Transaction` entities using EF Core (10) and NDbGate against SQL Server LocalDB, measured with BenchmarkDotNet (0.15.8, 1 warmup + 5 iterations). Test project is in the Repo (`PerformanceTest`).

Environment: .NET 10.0.13, Windows 11, AMD Ryzen AI 9 HX 370.

#### Results (mean time)

| Operation | Entities | EF Core 10 | NDbGate    |
|-----------|---------:|-----------:|-----------:|
| Insert    | 100      |    79.4 ms |   132.7 ms |
| Insert    | 1000     |   658.1 ms | 1,137.8 ms |
| Insert    | 5000     | 1,903.9 ms | 4,871.6 ms |
| Query     | 100      |   188.7 ms |   228.8 ms |
| Query     | 1000     | 1,112.9 ms | 1,590.4 ms |
| Query     | 5000     | 6,058.2 ms | 6,751.3 ms |
| Update    | 100      |   285.0 ms |   232.6 ms |
| Update    | 1000     | 1,819.4 ms | 1,559.4 ms |
| Update    | 5000     | 8,397.3 ms | 8,957.2 ms |
| Delete    | 100      |   279.7 ms |   153.1 ms |
| Delete    | 1000     | 1,408.4 ms | 2,126.6 ms |
| Delete    | 5000     | 4,764.3 ms | 8,766.1 ms |

#### Results (entities per second, 5000 entities)

| Operation | EF Core 10 | NDbGate |
|-----------|-----------:|--------:|
| Insert    | 2626       | 1026    |
| Query     | 825        | 741     |
| Update    | 595        | 558     |
| Delete    | 1049       | 570     |

### License
GNU GPL V3