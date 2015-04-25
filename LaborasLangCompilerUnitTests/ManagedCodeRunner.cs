using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace LaborasLangCompilerUnitTests
{
    internal class ManagedCodeRunner
    {
        public static string CreateProcessAndRun(string exePath, string[] arguments, string stdIn = "", int timeOutInMilliseconds = 500)
        {
            timeOutInMilliseconds = Debugger.IsAttached ? Timeout.Infinite : timeOutInMilliseconds;
            stdIn = stdIn != null ? stdIn : string.Empty;
            string stdOut = string.Empty;
            string exceptionText = string.Empty;

            bool testTimedOut = false;
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
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception e)
                {
                    exceptionText = e.ToString();
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
                testTimedOut = true;
                testThread.Abort();
                testThread.Join();
            }

            if (testTimedOut)
            {
                throw new TimeoutException(stdOut);
            }

            if (exitCode != 0)
            {
                var message = new StringBuilder(stdOut);

                if (!string.IsNullOrEmpty(exceptionText))
                {
                    message.AppendFormat("{0}{1} threw an exception: {2}", Environment.NewLine, Path.GetFileName(exePath), exceptionText);
                }

                throw new Exception(message.ToString());
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
