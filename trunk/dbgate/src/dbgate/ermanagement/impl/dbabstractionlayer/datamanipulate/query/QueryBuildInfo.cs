using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query
{
    public class QueryBuildInfo
    {
       	private readonly QueryExecInfo _execInfo;
  	 	private readonly Dictionary<string,object> _aliases;
	  	 	
  	 	public QueryBuildInfo()
  	 	{
  	 		_execInfo = new QueryExecInfo();
  	 		_aliases = new Dictionary<string,object>();
  	 	}
	  	 	
  	 	public QueryExecInfo ExecInfo
  	 	{
  	 		get {return _execInfo;}
  	 	}
  	 	
  	 	public Dictionary<string,object> Aliases
  	 	{
  	 		get { return _aliases;}
  	 	}
	  	 	
  	 	public void AddTypeAlias(string alias,Type entityType)
  	 	{
  	 		_aliases.Add(alias,entityType);
  	 	}
	  	 	
  	 	public void AddQueryAlias(string alias,ISelectionQuery query)
  	 	{
  	 		_aliases.Add(alias,query);
  	 	}
  	 	
  	 	public void AddUnionAlias(String alias)
		{
			_aliases.Add(alias,"UNION_" + alias);
		}
    }
}
