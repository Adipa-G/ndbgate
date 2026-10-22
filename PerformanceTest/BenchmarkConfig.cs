using Microsoft.Data.SqlClient;

namespace PerformanceTest
{
    public static class BenchmarkConfig
    {
        private const string Prefix = "Data Source=(localdb)\\MSSQLLocalDB;Integrated security=SSPI;MultipleActiveResultSets=true";

        public const string EfDb = "BenchmarkTestEF";
        public const string NDbGateDb = "BenchmarkTestNDbGate";

        public static readonly string EfConnectionString = $"{Prefix};database={EfDb}";
        public static readonly string NDbGateConnectionString = $"{Prefix};database={NDbGateDb}";

        public static void EnsureDbExists(string dbName)
        {
            using var conn = new SqlConnection($"{Prefix};database=master");
            conn.Open();
            using var cmd = new SqlCommand(
                $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}') CREATE DATABASE [{dbName}]",
                conn);
            cmd.ExecuteNonQuery();
        }
    }
}
