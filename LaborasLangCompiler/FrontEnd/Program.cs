using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.FrontEnd
{
    class Program
    {
        internal static int Main(params string[] args)
        {
            try
            {
                Errors.Clear();
                var compilerArgs = CompilerArguments.Parse(args);
                AssemblyRegistry.Create(compilerArgs.References);
                var assembly = new AssemblyEmitter(compilerArgs);

                ProjectParser.ParseAll(assembly, compilerArgs.SourceFiles, true);

                assembly.Save();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Compilation failed. Aborting.");

                if (Debugger.IsAttached)
                {
                    Console.ReadKey();
                }

                return -1;
            }

            return 0;
        }
    }
}
