using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbGate.Exceptions.Common;
using DbGate.Utility;
using log4net;
using log4net.Core;

namespace DbGate.ErManagement.ErMapper
{
    public class Transaction : ITransaction
    {
        private Guid transactionId;
	    private IDbGate dbGate;
	    private ITransactionFactory factory;
	    private IDbTransaction transaction;
        private IDbConnection connection;
	
	    public Transaction(ITransactionFactory factory, IDbTransaction transaction)
	    {
	        this.transactionId = Guid.NewGuid();
	        this.factory = factory;
	        this.transaction = transaction;
	        this.connection = transaction.Connection;
	        this.dbGate = factory.DbGate;
	    }
	
	    public ITransactionFactory Factory => factory;

        public Guid TransactionId => transactionId;

        public IDbConnection Connection => connection;

        public IDbGate DbGate => dbGate;

        public bool Closed
	    {
            get
            {
                try
	            {
	                return Connection == null 
                        || Connection != null && Connection.State == ConnectionState.Closed;
	            }
	            catch (Exception e)
	            {
                    LogManager.GetLogger(typeof (Transaction)).Fatal(e.Message, e);
	            }
	            return true;
	        }
	    }

        
	    public void Commit()
	    {
	        try
	        {
	            transaction.Commit();
	        }
	        catch (Exception e)
	        {
	            throw new TransactionCommitFailedException(String.Format("Unable to commit the transaction {0}"
                    ,transactionId.ToString()),e);
	        }    
        }
	
	    public void RollBack()
        {
	        try
	        {
	            transaction.Rollback();
	        }
	        catch (Exception e)
	        {
	            throw new TransactionRollbackFailedException(String.Format("Unable to rollback the transaction {0}"
	                ,transactionId.ToString()),e);
	        }
	    }
	
	    public void Close()
	    {
	        try
	        {
	            factory = null;
	            connection?.Close();
	            connection?.Dispose();
	        }
	        catch (Exception e)
	        {
	                throw new TransactionCloseFailedException(String.Format("Unable to close the transaction {0}"
	                    ,transactionId.ToString()),e);
	        }
	    }

        public IDbCommand CreateCommand()
        {
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            return cmd;
        }
    }
}
