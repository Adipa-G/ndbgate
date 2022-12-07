using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Text;

namespace PerformanceTest.EF
{
    public class InsertUpdateInterceptor : IDbCommandInterceptor
    {
        public virtual void NonQueryExecuting(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogCommand(command);
        }

        public virtual void ReaderExecuting(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            // this will capture all SELECT queries if you care about them..
            // however it also captures INSERT statements as well 
            LogCommand(command);
        }

        public virtual void ScalarExecuting(
         DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogCommand(command);
        }


        private void LogCommand(DbCommand dbCommand)
        {
            var commandText = new StringBuilder();

            commandText.AppendLine("-- New statement generated: " + System.DateTime.Now.ToString());
            commandText.AppendLine();

            // as the command has a bunch of parameters, we need to declare
            // those parameters here so the SQL will execute properly

            foreach (DbParameter param in dbCommand.Parameters)
            {
                var sqlParam = (SqlParameter)param;

                commandText.AppendLine(String.Format("DECLARE {0} {1} {2}",
                                                        sqlParam.ParameterName,
                                                        sqlParam.SqlDbType.ToString().ToLower(),
                                                        GetSqlDataTypeSize(sqlParam)));

                var escapedValue = sqlParam.SqlValue.ToString().Replace("'", "''");
                commandText.AppendLine(String.Format("SET {0} = '{1}'", sqlParam.ParameterName, escapedValue));
                commandText.AppendLine();
            }

            commandText.AppendLine(dbCommand.CommandText);
            commandText.AppendLine("GO");
            commandText.AppendLine();
            commandText.AppendLine();

            System.IO.File.AppendAllText("outputfile.sql", commandText.ToString());
        }

        private string GetSqlDataTypeSize(SqlParameter param)
        {
            if (param.Size == 0)
            {
                return "";
            }

            if (param.Size == -1)
            {
                return "(MAX)";
            }

            return "(" + param.Size + ")";
        }


        // To implement the IDbCommandInterceptor interface you need to also implement these methods like so

        public void NonQueryExecuted(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void ReaderExecuted(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        public void ScalarExecuted(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }
    }
}
