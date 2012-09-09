using System;
using System.Data;

namespace dbgate.ermanagement
{
    public interface IQuerySelection
    {
        Object Retrieve(IDataReader reader);
    }
}