using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests
{
    static class PEVerifyRunner
    {
        private static uint[] CodesToIgnore = new uint[]
        {
            0x8013185D
        };

        private static string _peverifyPath;
        private static string PEVerifyPath
        {
            get
            {
                if (_peverifyPath == null)
                {
                    var toolsPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Microsoft SDKs\Windows\v8.1A\WinSDK-NetFx40Tools", 
                       "InstallationFolder", null);

                    if (toolsPath == null)
                    {
                        throw new Exception("Can't find Windows SDK Tools folder!");
                    }

                    _peverifyPath = Path.Combine(toolsPath, "PEVerify.exe");
                }

                return _peverifyPath;
            }
        }


        public static void Run(string targetAssembly)
        {
            string arguments;

            if (CodesToIgnore.Length > 0)
            {
                arguments = string.Format(@"/verbose /hresult /ignore={1} ""{0}""", targetAssembly,
                    string.Join(",", CodesToIgnore.Select(errorCode => errorCode.ToString("X"))));
            }
            else
            {
                arguments = string.Format(@"/verbose /hresult ""{0}""", targetAssembly);
            }

            var startInfo = new ProcessStartInfo(PEVerifyPath, arguments);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            var process = new Process { StartInfo = startInfo };

            process.OutputDataReceived += (sender, args) => stdout.AppendLine(args.Data);
            process.ErrorDataReceived += (sender, args) => stderr.AppendLine(args.Data);

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();

            var output = stdout.ToString() + "\r\n" + stderr.ToString();

            if (process.ExitCode != 0)
            {
                var lines = output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // First 2 lines are logos, last one is conclusion
                var errorLines = lines.Skip(2).Take(lines.Length - 3).ToArray();

                if (errorLines.Length > 0)
                {
                    throw new Exception(string.Format("\r\n\r\nPEVerify.exe {0}\r\nfailed with error code {1}: \r\n{2}",
                        arguments, process.ExitCode, string.Join("\r\n", errorLines)));
                }
            }
        }
    }
}
