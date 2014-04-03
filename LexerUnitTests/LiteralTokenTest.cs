using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LaborasLangCompiler.LexingTools;
using NPEG;
using System.Text;

namespace LexerUnitTests
{
    [TestClass]
    public class LiteralTokenTest
    {
        [TestMethod]
        public void TestIntegerLiteralToken()
        {            
            var source = "foo = 1;";
            AstNode tree = Lexer.MakeTree(source);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: Literal: IntegerLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringLiteralToken()
        {
            var source = @"foo = 'bar';";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: Literal: StringLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFloatLiteralToken()
        {
            var source = "foo = 1.1;";
            AstNode tree = Lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: Literal: FloatLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
