using BenchmarkDotNet.Running;
using PerformanceTest.EF;
using PerformanceTest.NDbGate;

namespace PerformanceTest
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(new[]
            {
                typeof(EfInsertBenchmark),
                typeof(EfQueryBenchmark),
                typeof(EfUpdateBenchmark),
                typeof(EfDeleteBenchmark),
                typeof(NDbGateInsertBenchmark),
                typeof(NDbGateQueryBenchmark),
                typeof(NDbGateUpdateBenchmark),
                typeof(NDbGateDeleteBenchmark),
            });
        }
    }
}