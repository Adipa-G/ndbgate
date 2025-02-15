using System;
using System.Data;
using Microsoft.Data.SqlClient;
using PerformanceTest.EF;
using PerformanceTest.NDbGate;

namespace PerformanceTest
{
    public class MainProgram
    {
        private static string connectionStringPrefix = "Data Source=(localdb)\\MSSQLLocalDB;Integrated security=SSPI;MultipleActiveResultSets=true";

        public static void Main(string[] args)
        {
            var dbName = $"Test_{DateTime.UtcNow.Ticks}"; 
            CreateDB(dbName);

            var connectionString = $"{connectionStringPrefix};database={dbName}";

            new NDbGatePerformanceCounter(connectionString, 5000).Start(1);   
            new EfPerformanceCounter(connectionString, 5000).Start(1);   
        }

        private static void CreateDB(string dbName)
        {
            SqlConnection myConn = new SqlConnection($"{connectionStringPrefix};database=master");

            var cmdTextTemplate = @"CREATE DATABASE {0} ON 
                        PRIMARY  
                        (NAME = {0}_Data, FILENAME = '{1}\Data\{0}.mdf', SIZE = 200MB, MAXSIZE = 2000MB, FILEGROWTH = 10%)
                        LOG ON 
                        (NAME = {0}_Log, FILENAME = '{1}\Data\{0}_Log.ldf', SIZE = 10MB, MAXSIZE = 50MB, FILEGROWTH = 10%)";
            var cmdText = string.Format(cmdTextTemplate, dbName, AppDomain.CurrentDomain.BaseDirectory);

            SqlCommand myCommand = new SqlCommand(cmdText, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                Console.WriteLine("DataBase is Created Successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
        }
    }
}