using System;
using LaborasLangCompiler.LexingTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class LoopTests
    {
        Lexer Lexer;
        [TestInitialize]
        public void Initialize()
        {
            this.Lexer = new Lexer();
        }

        [TestMethod]
        public void TestWhileLoop()
        {
            string source = "while(condition){function();}";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: Loop: (Condition: Value: Sum: Product: Symbol, CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
