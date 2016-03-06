﻿using System.Threading;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            Integrator.DoProcess(@"Y:\dev\community\github\ndbgate\DbGateTestApp", @"Y:\dev\community\github\ndbgate\DbGateTestApp\wikigen");

            SimpleExample.SimpleExample.DoTest();
            InheritanceExample.InheritanceExample.DoTest();
            One2OneExample.One2OneExample.DoTest();
            One2ManyExample.One2ManyExample.DoTest();
            ComplexExample.ComplexExample.DoTest();
            Thread.Sleep(25000);
        }
    }
}