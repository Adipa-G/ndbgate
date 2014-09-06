using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGate
{
    public interface ITable
    {
        string TableName { get; set; }
        
        UpdateStrategy UpdateStrategy { get; set; }

        VerifyOnWriteStrategy VerifyOnWriteStrategy { get; set; }

        DirtyCheckStrategy DirtyCheckStrategy { get; set; }
    }
}
