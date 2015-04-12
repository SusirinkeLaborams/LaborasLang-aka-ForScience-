using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
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

            if (compilerArgs == null)
                return FailCompilation();

            AssemblyRegistry.Create(compilerArgs.References);
            var assembly = new AssemblyEmitter(compilerArgs);

            ProjectParser.ParseAll(assembly, compilerArgs, true);

            if (Errors.Reported.Count > 0)
                return FailCompilation();

            assembly.Save();
            return 0;
        }

        private static int FailCompilation()
        {
            Console.WriteLine("Compilation failed. Aborting.");

            if (Debugger.IsAttached)
                Debugger.Break();

            return -1;
        }
    }
}
