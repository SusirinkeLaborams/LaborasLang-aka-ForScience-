using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompilerUnitTests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    [TestClass]
    public class SomeRandomTest
    {
        private Lexer lexer;
        private CompilerArguments compilerArgs;
        [TestInitialize]
        public void Init()
        {
            compilerArgs = CompilerArguments.Parse(new[] { "test.ll" });
            lexer = new Lexer();
        }
        [TestMethod]
        public void TestSerialization()
        {
            var registry = new AssemblyRegistry(compilerArgs.References);
            var assembly = new AssemblyEmitter(compilerArgs, registry);
            string source = "int a = 5; \nint b = a;";
            var bytes = SourceReader.ReadSource(source);
            var tree = lexer.MakeTree(bytes);
            TreeSerializer.Serialize("test.xml", tree);
            tree = TreeSerializer.Deserialize("test.xml");
            Parser parser = new Parser(assembly, registry, tree, bytes, "test");
        }
    }
}
