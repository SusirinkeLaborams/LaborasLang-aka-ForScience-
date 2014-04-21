using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LaborasLangCompiler.LexingTools;
using NPEG;
using System.Text;
using LaborasLangCompilerUnitTests.ILTests;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class LiteralTokenTest : TestBase
    {

        [TestMethod]
        public void TestIntegerLiteralToken()
        {
            var source = "foo = 1;";
            AstNode tree = lexer.MakeTree(source);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNegativeIntegerToken()
        {
            var source = "foo = -1;";
            AstNode tree = lexer.MakeTree(source);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: (PrefixOperator, Literal: IntegerLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringLiteralToken()
        {
            var source = @"foo = 'bar';";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: StringLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestBooleanLiteralTrue()
        {
            var source = @"foo = true;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: BooleanLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestBooleanLiteralFalse()
        {
            var source = @"foo = false;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: BooleanLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringLiteralTokenDoubleQuote()
        {
            var source = @"foo = ""bar"";";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: StringLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFloatLiteralToken()
        {
            var source = "foo = 1.1;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: FloatLiteral), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
