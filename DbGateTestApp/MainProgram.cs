using DbGateTestApp.DocGenerate;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DbGateTestApp
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            GenerateDocumentation();

            SimpleExample.SimpleExample.DoTest();
            InheritanceExample.InheritanceExample.DoTest();
            One2OneExample.One2OneExample.DoTest();
            One2ManyExample.One2ManyExample.DoTest();
            ComplexExample.ComplexExample.DoTest();
            Thread.Sleep(25000);
        }

        private static void GenerateDocumentation()
        {
            var projectDirectory = GetProjectDirectory();

            if (string.IsNullOrEmpty(projectDirectory))
            {
                Console.WriteLine("Warning: Could not determine project directory. Skipping documentation generation.");
                return;
            }

            var outputDirectory = Path.Combine(projectDirectory, "wikigen");

            // Ensure output directory exists
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Console.WriteLine($"Processing documentation from: {projectDirectory}");
            Console.WriteLine($"Output directory: {outputDirectory}");

            Integrator.DoProcess(projectDirectory, outputDirectory);
        }

        private static string GetProjectDirectory()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            // Navigate up from bin\Debug\net8.0 (or Release) to project root
            var projectDirectory = Directory.GetParent(assemblyDirectory)?.Parent?.Parent?.FullName;

            if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
            {
                Console.WriteLine($"Warning: Could not determine project directory from assembly location: {assemblyLocation}");
                return null;
            }

            return projectDirectory;
        }
    }
}