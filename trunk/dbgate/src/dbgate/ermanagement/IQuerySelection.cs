using System;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IQuerySelection
    {
		QuerySelectionExpressionType SelectionExpressionType { get; }
    }
}