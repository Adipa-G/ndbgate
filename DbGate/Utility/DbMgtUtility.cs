﻿using System;
using System.Data;
using log4net;

namespace DbGate.Utility
{
    public class DbMgtUtility
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
                    LogManager.GetLogger(typeof (DbMgtUtility)).Fatal("Exception during closing connection", e);
                }
            }
        }

        public static void Close(ITransaction tx)
        {
            if (tx != null)
            {
                try
                {
                    if (!tx.Closed)
                    {
                        tx.Close();
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof(DbMgtUtility)).Fatal("Exception during closing transaction", e);
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
                    LogManager.GetLogger(typeof (DbMgtUtility)).Fatal("Exception during cancelling dbcpmmand", e);
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
                    LogManager.GetLogger(typeof (DbMgtUtility)).Fatal("Exception during closing data reader", e);
                }
            }
        }
    }
}