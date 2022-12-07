/*
 * Created by SharpDevelop.
 * User: adipa_000
 * Date: 5/27/2014
 * Time: 11:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From
{
    public class AbstractSubQueryFrom : IAbstractFrom
    {
        public ISelectionQuery Query { get; set; }
        public string Alias { get; set; }

        #region IAbstractFrom Members

        public QueryFromExpressionType FromExpressionType => QueryFromExpressionType.Query;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            var result = dbLayer.DataManipulate().ProcessQuery(buildInfo, Query.Structure);
            var sql = "(" + result.ExecInfo.Sql + ")";
            if (!string.IsNullOrEmpty(Alias))
            {
                sql = sql + " as " + Alias;
                buildInfo.AddQueryAlias(Alias, Query);
            }
            return sql;
        }

        #endregion
    }
}