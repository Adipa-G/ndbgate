using System;
using PerformanceTest.EF;
using PerformanceTest.NDbGate;

namespace PerformanceTest
{
    public class MainProgram
    {
        //private static string connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename={AppDomain.CurrentDomain.BaseDirectory}\\Data\\Test.mdf;Integrated Security=True;Connect Timeout=30;";
        private static string connectionString = $"Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog={AppDomain.CurrentDomain.BaseDirectory}\\Data\\Test.mdf;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=true";

        public static void Main(string[] args)
        {
            new NDbGatePerformanceCounter(connectionString,5000).Start(1);   
            new EfPerformanceCounter(connectionString,5000).Start(1);   
        }
    }
}