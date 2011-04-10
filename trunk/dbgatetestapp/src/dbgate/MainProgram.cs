using System.Threading;
using dbgatetestapp.dbgate.complexexample;
using dbgatetestapp.dbgate.inheritanceexample;
using dbgatetestapp.dbgate.one2manyexample;
using dbgatetestapp.dbgate.one2oneexample;
using dbgatetestapp.dbgate.simpleexample;

namespace dbgatetestapp.dbgate
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            SimpleExample.DoTest();
            InheritanceExample.DoTest();
            One2OneExample.DoTest();
            One2ManyExample.DoTest();
            ComplexExample.DoTest();
            Thread.Sleep(25000);
        }
    }
}
