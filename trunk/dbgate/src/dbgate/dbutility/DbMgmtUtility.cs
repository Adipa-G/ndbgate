using System;
using System.Data;
using log4net;

namespace dbgate.dbutility
{
    public class DbMgmtUtility
    {
        public static void Close(IDbConnection con)
        {
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Closed)
                    {
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof (DbMgmtUtility)).Fatal("Exception during closing connection", e);
                }
            }
        }

        public static void Close(IDbCommand dc)
        {
            if (dc != null)
            {
                try
                {
                    dc.Cancel();
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof (DbMgmtUtility)).Fatal("Exception during cancelling dbcpmmand", e);
                }
            }
        }

        public static void Close(IDataReader reader)
        {
            if (reader != null)
            {
                try
                {
                    reader.Close();
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof (DbMgmtUtility)).Fatal("Exception during closing data reader", e);
                }
            }
        }
    }
}