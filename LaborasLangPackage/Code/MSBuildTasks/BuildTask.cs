using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LaborasLangPackage.MSBuildTasks
{
    public class BuildTask : Task
    {
        [Required]
        public string ToolsetPath { get; set; }

        [Required]
        public string[] SourceFiles { get; set; }

        [Required]
        public string[] References { get; set; }

        [Required]
        public string OutputType { get; set; }

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public bool EnableOptimizations { get; set; }

        public override bool Execute()
        {
            string compilerOutput;
            var compilerPath = Path.Combine(ToolsetPath, "LaborasLangCompiler.exe");

            return RunExecutable(compilerPath, GetCompilerArgs());
        }

        private bool RunExecutable(string exe, IEnumerable<string> args)
        {
            var argumentLine = args.Aggregate((x, y) => x + " " + y);
            var startInfo = new ProcessStartInfo(exe, argumentLine);

            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var process = Process.Start(startInfo);
            process.OutputDataReceived += (sender, stdOut) =>
                {
                    if (stdOut.Data != null)
                        Log.LogCommandLine(MessageImportance.High, stdOut.Data);
                };
            process.BeginOutputReadLine();
            
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private IEnumerable<string> GetCompilerArgs()
        {
            var args = new List<string>();

            foreach (var sourceFile in SourceFiles)
            {
                args.Add(sourceFile);
            }

            foreach (var reference in References)
            {
                args.Add("/ref:" + reference);
            }

            var extension = OutputType.ToLowerInvariant() == "library" ? ".dll" : ".exe";
            args.Add("/out:" + Path.Combine(OutputPath, AssemblyName + extension));
            args.Add("/" + OutputType);

            if (!EnableOptimizations)
            {
                args.Add("/debug");
            }

            return args;
        }
    }
}
