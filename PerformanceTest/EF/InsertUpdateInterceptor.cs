using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PerformanceTest.EF
{
    public class InsertUpdateInterceptor : IDbCommandInterceptor
    {
        public InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            LogCommand(command);
            return result;
        }

        public InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            LogCommand(command);
            return result;
        }

        public InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            LogCommand(command);
            return result;
        }

        public ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            LogCommand(command);
            return new (result);
        }

        public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            LogCommand(command);
            return new(result);
        }

        public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            LogCommand(command);
            return new(result);
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
    }
}
