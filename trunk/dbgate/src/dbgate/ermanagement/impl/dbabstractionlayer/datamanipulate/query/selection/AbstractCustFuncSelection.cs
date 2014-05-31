/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/31/2014
 * Time: 11:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using dbgate.ermanagement.query;

namespace dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractCustFuncSelection : BaseColumnOperation
	{
		public AbstractCustFuncSelection()
		{
		}
		
		public string Function 
		{
			get{ return _function;}
			set{ _function = value;}
		}
		
		public override QuerySelectionExpressionType SelectionType 
		{
			get 
			{
				return QuerySelectionExpressionType.CUST_FUNC;
			}
		}
	}
}
