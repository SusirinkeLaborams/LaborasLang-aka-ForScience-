﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests
{
    static class PEVerifyRunner
    {
        private static uint[] CodesToIgnore = new uint[]
        {
            0x80131854,     // Expected native int, found method on stack (happens when creating functor)
            0x80131861,     // Expected pointer to function on a stack, found native int (happens in Functor.AsDelegate when constructing a delegate
            0x8013186E,     // Instruction cannot be verified (calli)
            0x801318BD,     // initlocals must be set for verifiable methods with one or more local variables.
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
            var arguments = string.Format(@"/verbose /hresult /ignore={1} ""{0}""", targetAssembly,
                string.Join(",", CodesToIgnore.Select(x => x.ToString("X"))));

            var startInfo = new ProcessStartInfo(PEVerifyPath, arguments);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var output = string.Join("\r\n\r\n", process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());

                throw new Exception(string.Format("\r\n\r\nPEVerify.exe {0}\r\nfailed with error code {1}: \r\n{2}", 
                    arguments, process.ExitCode, output));
            }
        }
    }
}