using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Lexer;

namespace LaborasLangCompiler.FrontEnd
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var compilerArgs = CompilerArguments.Parse(args);
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
