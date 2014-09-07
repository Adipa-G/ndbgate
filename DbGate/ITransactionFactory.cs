using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate
{
    public interface ITransactionFactory
    {
        ITransaction CreateTransaction();

        IDbGate DbGate { get; }
    }
}
