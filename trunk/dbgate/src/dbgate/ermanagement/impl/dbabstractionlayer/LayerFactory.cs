using dbgate.dbutility;

namespace dbgate.ermanagement.impl.dbabstractionlayer
{
    public class LayerFactory
    {
        public static IDbLayer CreateLayer(int dbType,IErLayerConfig config)
        {
            switch (dbType)
            {
                case DbConnector.DbAccess:
                    return new AccessDbLayer(config);
                case DbConnector.DbMysql:
                    return new MySqlDbLayer(config);
                case DbConnector.DbSqllite:
                    return new SqlLiteDbLayer(config);
                default:
                    return new DefaultDbLayer(config);
            }
        }
    }
}
