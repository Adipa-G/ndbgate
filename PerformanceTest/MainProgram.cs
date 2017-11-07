using PerformanceTest.EF;
using PerformanceTest.NDbGate;

namespace PerformanceTest
{
    public class MainProgram
    {
        private static string connectionString =
            "Data Source=localhost;Integrated Security=SSPI;Initial Catalog=DbGate";

        public static void Main(string[] args)
        {
            new NDbGatePerformanceCounter(connectionString,100).Start(1);   
            new EFPerformanceCounter(connectionString,100).Start(1);   
        }
    }
}