using System.Threading;

namespace DbGateTestApp
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            SimpleExample.SimpleExample.DoTest();
            InheritanceExample.InheritanceExample.DoTest();
            One2OneExample.One2OneExample.DoTest();
            One2ManyExample.One2ManyExample.DoTest();
            ComplexExample.ComplexExample.DoTest();
            Thread.Sleep(25000);
        }
    }
}