using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompilerUnitTests.ILTests;
using LaborasLangCompilerUnitTests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    [TestClass]
    public class ParserTests : TestBase
    {
        private const string path = @"..\..\ParserTests\SerializedLexerTrees\";
        [TestMethod]
        public void FieldDeclarationTest()
        {
            string source = "auto a = 5; int b; int c = 10;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b, System.Int32 c = (Literal: System.Int32 10) Methods: )";
            TestParser(source, expected, "Declarations", false);
        }
        [TestMethod]
        public void TypeExceptionTest()
        {
            string source = "int a = 0.0;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Single 0) Methods: )";
            try
            {
                TestParser(source, expected, "TypeFailure", false);
            }
            catch(TypeException)
            {
                //should throw
                return;
            }
            //no type error
            Assert.Fail();
        }
        private void TestParser(string source, string expected, string name, bool lex)
        {
            var compilerArgs = CompilerArguments.Parse(new[] { name + ".ll" });
            var assembly = new AssemblyEmitter(compilerArgs);
            var bytes = SourceReader.ReadSource(source);
            AstNode tree;
            if(lex)
            {
                tree = lexer.MakeTree(bytes);
                TreeSerializer.Serialize(path + name + ".xml", tree);
            }
            else
            {
                tree = TreeSerializer.Deserialize(path + name + ".xml");
            }
            Parser parser = new Parser(assembly, tree, bytes, "test");
            string result = parser.Root.Print();
            Assert.AreEqual(expected, result);
        }
    }
}
