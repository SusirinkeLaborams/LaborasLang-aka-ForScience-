using LaborasLangCompiler.LexingTools;
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
using NPEG;

namespace LaborasLangCompiler.FrontEnd
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var compilerArgs = CompilerArguments.Parse(args);
                var lexer = new Lexer();
                AssemblyRegistry.Create(compilerArgs.References);
                var assembly = new AssemblyEmitter(compilerArgs);
                
                foreach (var file in compilerArgs.SourceFiles)
                {
                    var bytes = FileReader.Read(file);
                    SymbolCounter.Load(file);
                    var tree = lexer.MakeTree(bytes);

#if DEBUG
                    PrintAst(tree, bytes);
#endif
                    var parser = new Parser.Parser(assembly, tree, bytes, System.IO.Path.GetFileNameWithoutExtension(file));
                }

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

        static void PrintAst(AstNode tree, ByteInputIterator tokens)
        {
            PrintAstNode(tree, tokens, "");
            PrintAst(tree, 1, tokens);
        }

        static void PrintAst(AstNode tree, int depth, ByteInputIterator tokens)
        {
            var tabs = new String('\t', depth);
            foreach (var child in tree.Children)
            {
                if (!child.Token.Name.Equals("Ws"))
                {
                    PrintAstNode(child, tokens, tabs);
                }
                PrintAst(child, depth + 1, tokens);
            }
        }

        static void PrintAstNode(AstNode node, ByteInputIterator tokens, string tabs)
        {
            var tokenValue = System.Text.Encoding.UTF8.GetString(tokens.Text(node.Token.Start, node.Token.End));
            tokenValue = tokenValue.Replace("\t", "").Replace("    ", "").Replace("\r\n", "");
            Debug.WriteLine(String.Format("{0}{1}: [{2}]", tabs, node.Token.Name, tokenValue));
        }
    }
}
