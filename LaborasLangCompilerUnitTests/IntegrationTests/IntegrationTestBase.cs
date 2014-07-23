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
            public string SourceFile { get; private set; }
            
            public string[] Arguments { get; set; }
            public string StdIn { get; set; }
            public string StdOut { get; set; }

            public IntegrationTestInfo(string sourceFile)
            {
                SourceFile = sourceFile;
            }

            public IntegrationTestInfo(string sourceFile, string expectedOutput)
            {
                SourceFile = sourceFile;
                StdOut = expectedOutput;
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

            var sourceFile = Path.Combine(IntegrationTestsPath, "SourceFiles", testInfo.SourceFile);
            
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
            CreateProcessAndRun("LaborasLangCompiler.exe", new[] { sourceFile, "/console", "/out:" + outPath }, null, kBuildTimeOut);
        }

        private void Run(string path, IntegrationTestInfo testInfo)
        {
            var stdout = CreateProcessAndRun(path, testInfo.Arguments, testInfo.StdIn, kRunTimeOut);
            Assert.AreEqual(testInfo.StdOut, stdout);
        }

        private string CreateProcessAndRun(string exePath, string[] arguments, string stdIn, int timeOutInMilliseconds)
        {
            var argumentLine = arguments != null ? arguments.Aggregate((x, y) => x + " " + y) : string.Empty;
            var startInfo = new ProcessStartInfo(exePath, argumentLine);

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var process = Process.Start(startInfo);

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            if (stdIn != null)
            {
                process.StandardInput.Write(stdIn);
            }

            bool timedOut = false;

            if (!process.WaitForExit(timeOutInMilliseconds))
            {
                process.Kill();
                timedOut = true;
            }

            stdoutTask.Wait();
            var stdout = stdoutTask.Result;

            if (timedOut)
            {
                throw new TimeoutException(stdout);
            }

            if (process.ExitCode != 0)
            {
                throw new Exception(stdout);
            }

            return stdout;
        }
    }
}
