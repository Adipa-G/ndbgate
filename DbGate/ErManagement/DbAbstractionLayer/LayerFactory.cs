namespace DbGate.ErManagement.DbAbstractionLayer
{
    public class LayerFactory
    {
        public static IDbLayer CreateLayer(int dbType, IDbGateConfig config)
        {
            switch (dbType)
            {
                case DefaultTransactionFactory.DbAccess:
                    return new AccessDbLayer(config);
                case DefaultTransactionFactory.DbMysql:
                    return new MySqlDbLayer(config);
                case DefaultTransactionFactory.DbSqllite:
                    return new SqlLiteDbLayer(config);
                case DefaultTransactionFactory.DbSqlServer:
                    return new SqlServerDbLayer(config);
                default:
                    return new DefaultDbLayer(config);
            }
        }
    }
}