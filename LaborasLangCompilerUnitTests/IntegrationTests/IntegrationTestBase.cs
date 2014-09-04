using LaborasLangCompilerUnitTests.ILTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
                PEVerifyRunner.Run(exePath);
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
            timeOutInMilliseconds = Debugger.IsAttached ? Timeout.Infinite : timeOutInMilliseconds;
            stdIn = stdIn != null ? stdIn : string.Empty;
            string stdOut = string.Empty;

            bool testFinished = false;
            int exitCode = -1;

            var testThread = new Thread(() =>
            {
                var testDomain = AppDomain.CreateDomain("Test Domain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
                var stdOutHelper = (RedirectStdStreamsHelper)testDomain.CreateInstanceAndUnwrap(
                    typeof(RedirectStdStreamsHelper).Assembly.FullName, 
                    typeof(RedirectStdStreamsHelper).FullName,
                    false,
                    BindingFlags.CreateInstance,
                    null,
                    new object[] { stdIn },
                    null,
                    null);

                try
                {
                    exitCode = testDomain.ExecuteAssembly(exePath, arguments);
                    testFinished = true;
                }
                catch (ThreadAbortException)
                {
                }
                finally
                {
                    stdOut = stdOutHelper.GetStdOut();
                    AppDomain.Unload(testDomain);
                }
            });

            testThread.Start();
            if (!testThread.Join(timeOutInMilliseconds))
            {
                testThread.Abort();
            }

            if (!testFinished)
            {
                throw new TimeoutException(stdOut);
            }
            
            if (exitCode != 0)
            {
                throw new Exception(stdOut);
            }

            return stdOut;
        }

        private class RedirectStdStreamsHelper : MarshalByRefObject
        {
            private StringWriter writer;
            private StringReader reader;

            public RedirectStdStreamsHelper(string stdIn)
            {
                writer = new StringWriter();
                reader = new StringReader(stdIn);

                Console.SetOut(StreamWriter.Synchronized(writer));                
                Console.SetIn(StreamReader.Synchronized(reader));
            }

            public string GetStdOut()
            {
                return writer.ToString();
            }
        }
    }
}
