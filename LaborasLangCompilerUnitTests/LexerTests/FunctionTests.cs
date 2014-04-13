using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using LaborasLangCompiler.LexingTools;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class FunctionTests
    {
        Lexer Lexer;
        [TestInitialize]
        public void Initialize()
        {
            this.Lexer = new Lexer();
        }

        [TestMethod]
        public void DeclareFunctionTest()
        {
            var source = @"int(int, bool) foo;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Declaration: (FunctionType: (Type: Symbol, ArgumentTypes: (Type: Symbol, Type: Symbol)), Symbol), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
