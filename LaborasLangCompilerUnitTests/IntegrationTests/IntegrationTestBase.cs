using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LaborasLangCompilerUnitTests.IntegrationTests
{
    public class IntegrationTestBase : TestBase
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
            public IEnumerable<string> AdditionalReferences { get; private set; }
            
            public string[] Arguments { get; set; }
            public string StdIn { get; set; }
            public string StdOut { get; set; }

            public IntegrationTestInfo(string sourceFile)
            {
                SourceFiles = new[] { sourceFile };
                AdditionalReferences = new string[0];
            }

            public IntegrationTestInfo(string sourceFile, IEnumerable<string> additionalReferences)
            {
                SourceFiles = new[] { sourceFile };
                AdditionalReferences = additionalReferences;
            }
            
            public IntegrationTestInfo(IEnumerable<string> sourceFiles, string expectedOutput)
            {
                SourceFiles = sourceFiles;
                StdOut = expectedOutput;
                AdditionalReferences = new string[0];
            }

            public IntegrationTestInfo(IEnumerable<string> sourceFiles, string expectedOutput, IEnumerable<string> additionalReferences)
            {
                SourceFiles = sourceFiles;
                StdOut = expectedOutput;
                AdditionalReferences = additionalReferences;
            }
        }

        public IntegrationTestBase() :
            base(false)
        {
        }

        protected void Test(string sourceFile, string expectedOutput)
        {
            Test(new IntegrationTestInfo(new[] { sourceFile }, expectedOutput));
        }

        protected void Test(IEnumerable<string> sourceFiles, string expectedOutput)
        {
            Test(new IntegrationTestInfo(sourceFiles, expectedOutput));
        }

        protected void Test(string sourceFile, string expectedOutput, IEnumerable<string> additionalReferences)
        {
            Test(new IntegrationTestInfo(new[] { sourceFile }, expectedOutput, additionalReferences));
        }

        protected void Test(IEnumerable<string> sourceFiles, string expectedOutput, IEnumerable<string> additionalReferences)
        {
            Test(new IntegrationTestInfo(sourceFiles, expectedOutput, additionalReferences));
        }

        protected void Test(IntegrationTestInfo testInfo)
        {
            var filePath = Path.Combine(TestBase.GetTestDirectory(), "TestExecutable");

            var exePath = filePath + ".exe";
            var pdbPath = filePath + ".pdb";

            var files = testInfo.SourceFiles.Select(file => Path.Combine(IntegrationTestsPath, "SourceFiles", file));
            var references = testInfo.AdditionalReferences.Select(reference => string.Format("/ref:{0}", reference));

            Build(files, references, exePath);
            PEVerifyRunner.Run(exePath);
            Run(exePath, testInfo);
        }

        private void Build(IEnumerable<string> sourceFiles, IEnumerable<string> references, string outPath)
        {
            var args = sourceFiles.Union(references).Union(new[] { "/console", "/out:" + outPath }).ToArray();
            ManagedCodeRunner.CreateProcessAndRun("LaborasLangCompiler.exe", args, null, kBuildTimeOut);
        }

        private void Run(string path, IntegrationTestInfo testInfo)
        {
            var stdout = ManagedCodeRunner.CreateProcessAndRun(path, testInfo.Arguments, testInfo.StdIn, kRunTimeOut);
            Assert.AreEqual(testInfo.StdOut.Trim(), stdout.Trim());
        }
    }
}
