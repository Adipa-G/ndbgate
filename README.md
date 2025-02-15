# Introduction
NDbGate is a high performance ORM. The framework provides granular level control over persistence/retrieval.

### Features
* .Net Standard 2.0
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

Inserting/ Quering/ Updating/ Deleting 5000 `Transaction` entities using EF (6.2) and NDbGate. Test project is in the Repo.

#### Results (entities per second)

##### EF (9)			
	Insertion :	8549			
	Querying :  6038			
	Update : 4360			
	Delete	: 5777			

#### NDbGate
	Insertion :	4055
	Querying :	2716
	Update : 2219
	Delete : 2219

### License
GNU GPL V3