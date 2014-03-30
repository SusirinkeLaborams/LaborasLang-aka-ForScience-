using LaborasLangCompiler.LexingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LaborasLangCompiler.ILTools;
using Mono.Cecil;

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

                EmitHelloWorld(compilerArgs);
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
                    tokenValue = tokenValue.Replace("\t", "").Replace("    ", "").Replace("\r\n", "");
                    Debug.WriteLine(String.Format("{0}{1}: [{2}]", tabs, child.Token.Name, tokenValue));
                }
                PrintAst(child, depth + 1, tokens);
            }
        }

        static void EmitHelloWorld(CompilerArguments compilerArgs)
        {
            var assemblyRegistry = new AssemblyRegistry(compilerArgs.References);
            var assembly = new AssemblyEmitter(compilerArgs);

            var type = TypeEmitter.CreateTypeDefinition(assembly, "", "Laboras");

            var method = new MethodEmitter(assemblyRegistry, type, "Main", assemblyRegistry.GetType("System.Void"), MethodAttributes.Static | MethodAttributes.Private);
            method.EmitHelloWorld();
            method.SetAsEntryPoint();

            assembly.Save();
        }
    }
}
