using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DbGateTest.Persist
{
    [TestFixture]
    public class TmpTest
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("setup");
        }

        [Test]
        public void Test()
        {
            Console.WriteLine("test");
        }
    }
}
