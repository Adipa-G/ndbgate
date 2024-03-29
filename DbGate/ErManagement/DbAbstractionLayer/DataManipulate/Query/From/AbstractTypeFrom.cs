using System;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.ErManagement.Query;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.Query.From
{
    public class AbstractTypeFrom : IAbstractFrom
    {
        public Type EntityType { get; set; }
        public string Alias { get; set; }

        #region IAbstractFrom Members

        public QueryFromExpressionType FromExpressionType => QueryFromExpressionType.EntityType;

        public string CreateSql(IDbLayer dbLayer, QueryBuildInfo buildInfo)
        {
            var entityInfo = CacheManager.GetEntityInfo(EntityType);
            var sql = entityInfo.TableInfo.TableName;

            if (!string.IsNullOrEmpty(Alias))
            {
                sql = sql + " as " + Alias;
                buildInfo.AddTypeAlias(Alias, EntityType);
            }
            return sql;
        }

        #endregion
    }
}