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
        private Guid _transactionId;
	    private IDbGate _dbGate;
	    private ITransactionFactory _factory;
	    private IDbTransaction _transaction;
	
	    public Transaction(ITransactionFactory factory, IDbTransaction transaction)
	    {
	        this._transactionId = Guid.NewGuid();
	        this._factory = factory;
	        this._transaction = transaction;
	        this._dbGate = factory.DbGate;
	    }
	
	    public ITransactionFactory Factory
	    {
	        get { return _factory; }
	    }
	
	    public Guid TransactionId
	    {
            get { return _transactionId; }
	    }
	
	    public IDbConnection Connection
	    {
	         get { return _transaction.Connection; }
	    }
	
	    public IDbGate DbGate
	    {
	        get { return _dbGate; }
	    }
	
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
	            _transaction.Commit();
	        }
	        catch (Exception e)
	        {
	            throw new TransactionCommitFailedException(String.Format("Unable to commit the transaction {0}"
                    ,_transactionId.ToString()),e);
	        }    
        }
	
	    public void RollBack()
        {
	        try
	        {
	            _transaction.Rollback();
	        }
	        catch (Exception e)
	        {
	            throw new TransactionRollbackFailedException(String.Format("Unable to rollback the transaction {0}"
	                ,_transactionId.ToString()),e);
	        }
	    }
	
	    public void Close()
	    {
	        try
	        {
	            _factory = null;
	            Connection?.Close();
	        }
	        catch (Exception e)
	        {
	                throw new TransactionCloseFailedException(String.Format("Unable to close the transaction {0}"
	                    ,_transactionId.ToString()),e);
	        }
	    }

        public IDbCommand CreateCommand()
        {
            var cmd = Connection.CreateCommand();
            cmd.Transaction = _transaction;
            return cmd;
        }
    }
}
