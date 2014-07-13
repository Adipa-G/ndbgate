using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate
{
    public interface ITransaction
    {
        ITransactionFactory Factory { get; }

        Guid TransactionId { get; }

        IDbConnection Connection { get; }

        IDbGate DbGate { get; }

        bool Closed { get; }

        void Commit();

        void RollBack();

        void Close();

        IDbCommand CreateCommand();
    }
}
