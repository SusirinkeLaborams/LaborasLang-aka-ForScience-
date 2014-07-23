using LaborasLangCompiler.LexingTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.IntegrationTests
{
    public class IntegrationTestBase
    {
        private const int kTimeOutInMilliseconds = 100;

        protected static Lexer lexer = new Lexer();
        
        protected class IntegrationTestInfo
        {
            public string SourceFile { get; private set; }
            public string ExpectedOutput { get; private set; }
            
            public string[] Arguments { get; set; }
            public string StdIn { get; set; }

            public IntegrationTestInfo(string sourceFile, string expectedOutput)
            {
                SourceFile = sourceFile;
                ExpectedOutput = expectedOutput;
            }
        }

        protected void Test(string sourceFile, string expectedOutput)
        {
            Test(new IntegrationTestInfo(sourceFile, expectedOutput));
        }

        protected void Test(IntegrationTestInfo testInfo)
        {
            var temp = Path.GetTempPath() + Guid.NewGuid().ToString();
            var exePath = temp + ".exe";
            var pdbPath = temp + ".pdb";

            var sourceFile = Path.Combine("..", "..", "IntegrationTests", "SourceFiles", testInfo.SourceFile);
            
            try
            {
                Build(sourceFile, exePath);
                Run(exePath, testInfo);
            }
            finally
            {
                if (File.Exists(exePath)) File.Delete(exePath);
                if (File.Exists(pdbPath)) File.Delete(pdbPath);
            }
        }

        private void Build(string sourceFile, string outPath)
        {
            LaborasLangCompiler.FrontEnd.Program.Main(sourceFile, "/console", "/out:" + outPath);
        }

        private void Run(string path, IntegrationTestInfo testInfo)
        {
            var argumentLine = testInfo.Arguments != null ? testInfo.Arguments.Aggregate((x, y) => x + y) : string.Empty;
            var startInfo = new ProcessStartInfo(path, argumentLine);

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var process = Process.Start(startInfo);

            if (testInfo.StdIn != null)
            {
                process.StandardInput.Write(testInfo.StdIn);
            }

            if (!process.WaitForExit(kTimeOutInMilliseconds))
            {
                process.Kill();
                throw new TimeoutException();
            }

            var stdout = process.StandardOutput.ReadToEnd();
            Assert.AreEqual(testInfo.ExpectedOutput, stdout);
        }
    }
}
