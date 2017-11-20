using PerformanceTest.EF;
using PerformanceTest.NDbGate;

namespace PerformanceTest
{
    public class MainProgram
    {
        private static string connectionString =
            "Data Source=localhost;Integrated Security=SSPI;MultipleActiveResultSets=True;Initial Catalog=DbGate";

        public static void Main(string[] args)
        {
            new NDbGatePerformanceCounter(connectionString,5000).Start(1);   
            new EFPerformanceCounter(connectionString,5000).Start(1);   
        }
    }
}