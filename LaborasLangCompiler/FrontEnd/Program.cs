using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Emitters;
using LaborasLangCompiler.Parser.Impl;
using System;
using System.Diagnostics;

namespace LaborasLangCompiler.FrontEnd
{
    class Program
    {
        internal static int Main(params string[] args)
        {
            Errors.Clear();
            var compilerArgs = CompilerArguments.Parse(args);
            AssemblyRegistry.Create(compilerArgs.References);
            var assembly = new AssemblyEmitter(compilerArgs);

            ProjectParser.ParseAll(new EmitterSource(assembly), compilerArgs.SourceFiles);

            if (Errors.Reported.Count == 0)
            {
                assembly.Save();
                return 0;
            }
            else
            {
                Console.WriteLine("Compilation failed. Aborting.");
                if (Debugger.IsAttached)
                {
                    Console.ReadKey();
                }
                return -1;
            }
        }
    }
}
