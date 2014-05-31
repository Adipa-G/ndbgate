/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/31/2014
 * Time: 11:39 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using dbgate.ermanagement.query;

namespace dbgate.dbgate.ermanagement.impl.dbabstractionlayer.datamanipulate.query.selection
{
	public class AbstractColumnSelection : BaseColumnOperation
	{
		public AbstractColumnSelection()
		{
		}
		
		public override QuerySelectionExpressionType SelectionType 
		{
			get 
			{
				return QuerySelectionExpressionType.COLUMN;
			}
		}
	}
}
