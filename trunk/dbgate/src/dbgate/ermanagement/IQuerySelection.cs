using System;
using System.Data;

namespace dbgate.ermanagement
{
    public interface IQuerySelection
    {
		QuerySelectionType SelectionType { get; }
    }
}