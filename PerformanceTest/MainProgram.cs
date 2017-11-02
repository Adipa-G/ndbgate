using PerformanceTest.NDbGate;

namespace PerformanceTest
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            new NDbGatePerformanceCounter().Start(10);   
        }
    }
}