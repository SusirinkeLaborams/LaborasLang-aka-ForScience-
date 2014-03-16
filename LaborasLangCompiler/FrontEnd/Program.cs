using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LaborasLangCompiler.FrontEnd
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var compilerArgs = CompilerArguments.Parse(args);
                foreach (var file in compilerArgs.SourceFiles)
                {
                    var bytes = FileReader.Read(file);
                    var tree = Lexer.MakeTree(bytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Compilation failed. Aborting.");

                return -1;
            }

            return 0;
        }
    }
}
