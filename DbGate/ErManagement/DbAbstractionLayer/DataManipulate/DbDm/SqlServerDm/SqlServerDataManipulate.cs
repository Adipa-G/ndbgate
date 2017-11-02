using System;
using System.Data;
using System.Linq;
using System.Text;

namespace DbGate.ErManagement.DbAbstractionLayer.DataManipulate.DbDm.SqlServerDm
{
    public class SqlServerDataManipulate : AbstractDataManipulate
    {
        public SqlServerDataManipulate(IDbLayer dbLayer) : base(dbLayer)
        {
        }

        protected override string FixUpQuery(string query)
        {
            var sb = new StringBuilder();
            var tokens = query.Split(new []{'?'},StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (i > 0)
                {
                    sb.Append($"@{i}");
                }
                sb.Append(token);
            }

            return sb.ToString();
        }

        protected override void SetToPreparedStatement(IDbCommand cmd,
            object obj,
            int parameterIndex,
            bool nullable,
            ColumnType columnType)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = "@" + parameterIndex;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = obj ?? DBNull.Value;

            cmd.Parameters.Add(parameter);
            parameter.DbType = ColumnTypeMapping.GetSqlType(columnType);
        }
    }
}