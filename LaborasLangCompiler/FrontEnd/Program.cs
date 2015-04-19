using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Impl;
using System;
using System.Diagnostics;
using System.Linq;

namespace LaborasLangCompiler.FrontEnd
{
    class Program
    {
        internal static int Main(params string[] args)
        {
            try
            {
                return Compile(args);
            }
            catch (Exception e)
            {
                var exceptionMessage = e.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Aggregate((x, y) => x + "r\r\n\t" + y);
                Console.WriteLine("Internal compiler error has occurred. Details for inquiring minds: {0}\t{1}", Environment.NewLine, exceptionMessage);
                return -2;
            }
        }

        private static int Compile(params string[] args)
        {
            Errors.Clear();

            var compilerArgs = CompilerArguments.Parse(args);

            if (compilerArgs.HasErrors)
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
