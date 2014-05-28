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
            string expected = "Root: Sentence: Loop: (Condition: Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol, CodeBlock: Sentence: (FunctionCall: (FullSymbol: Symbol, Arguments), EndOfSentence))";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
