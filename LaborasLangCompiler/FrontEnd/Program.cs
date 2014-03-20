using LaborasLangCompiler.LexingTools;
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
            //Parser.Types.Testing.TestTypes();
            try
            {
                var compilerArgs = CompilerArguments.Parse(args);
                foreach (var file in compilerArgs.SourceFiles)
                {
                    var bytes = FileReader.Read(file);
                    var tree = Lexer.MakeTree(bytes);
                    PrintAst(tree, 1, bytes);

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

        static void PrintAst(NPEG.AstNode Tree, int depth, NPEG.ByteInputIterator tokens)
        {
            var tabs = new String('\t', depth);
            foreach (var child in Tree.Children)
            {
                if (!child.Token.Name.Equals("Ws"))
                {
                    var tokenValue = System.Text.Encoding.UTF8.GetString(tokens.Text(child.Token.Start, child.Token.End));
                    Console.WriteLine(String.Format("{0}{1}: [{2}]\n", tabs, child.Token.Name, tokenValue.Replace("\r\n","")));
                }
                foreach (var grandson in child.Children)
                {
                    PrintAst(grandson, depth + 1, tokens);
                }
            }
        }
    }
}
