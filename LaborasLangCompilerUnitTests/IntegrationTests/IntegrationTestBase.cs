using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaborasLangCompilerUnitTests.IntegrationTests
{
    public class IntegrationTestBase
    {
        private const int kBuildTimeOut = 4000;
        private const int kRunTimeOut = 500;

        protected string IntegrationTestsPath
        {
            get
            {
                return Path.Combine("..", "..", "IntegrationTests");
            }
        }

        protected class IntegrationTestInfo
        {
            public IEnumerable<string> SourceFiles { get; private set; }
            
            public string[] Arguments { get; set; }
            public string StdIn { get; set; }
            public string StdOut { get; set; }

            public IntegrationTestInfo(string sourceFile)
            {
                SourceFiles = new[] { sourceFile };
            }

            public IntegrationTestInfo(string sourceFile, string expectedOutput)
            {
                SourceFiles = new[] { sourceFile };
                StdOut = expectedOutput;
            }

            public IntegrationTestInfo(IEnumerable<string> sourceFiles, string expectedOutput)
            {
                SourceFiles = sourceFiles;
                StdOut = expectedOutput;
            }
        }

        protected void Test(string sourceFile, string expectedOutput)
        {
            Test(new IntegrationTestInfo(sourceFile, expectedOutput));
        }

        protected void Test(IEnumerable<string> sourceFiles, string expectedOutput)
        {
            Test(new IntegrationTestInfo(sourceFiles, expectedOutput));
        }

        protected void Test(IntegrationTestInfo testInfo)
        {
            var filePath = Path.Combine(TestBase.GetTestDirectory(), "TestExecutable");

            var exePath = filePath + ".exe";
            var pdbPath = filePath + ".pdb";

            var files = testInfo.SourceFiles.Select(file => Path.Combine(IntegrationTestsPath, "SourceFiles", file));
            
            Build(files, exePath);
            PEVerifyRunner.Run(exePath);
            Run(exePath, testInfo);
        }

        private void Build(IEnumerable<string> sourceFiles, string outPath)
        {
            ManagedCodeRunner.CreateProcessAndRun("LaborasLangCompiler.exe", sourceFiles.Union(new[] { "/console", "/out:" + outPath }).ToArray(), null, kBuildTimeOut);
        }

        private void Run(string path, IntegrationTestInfo testInfo)
        {
            var stdout = ManagedCodeRunner.CreateProcessAndRun(path, testInfo.Arguments, testInfo.StdIn, kRunTimeOut);
            Assert.AreEqual(testInfo.StdOut.Trim(), stdout.Trim());
        }
    }
}
