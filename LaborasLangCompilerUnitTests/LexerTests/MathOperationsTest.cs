using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompilerUnitTests.ILTests;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class MathOperationsTest : TestBase
    {

        [TestMethod]
        public void TestUnaryNegation()
        {
            var source = "foo = -bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Sum: Product: SuffixNode: PrefixNode: (PrefixOperator, Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddition()
        {
            var source = @"foo = 1 + bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: SuffixNode: PrefixNode: Literal: IntegerLiteral, SumOperator, Product: SuffixNode: PrefixNode: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubtraction()
        {
            var source = @"foo = bar - 1;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: Symbol, SumOperator, Product: Literal: IntegerLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultiplication()
        {
            var source = @"foo = bar * 1.5;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: (Symbol, MultiplicationOperator, Literal: FloatLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDivision()
        {
            var source = @"foo = 1.5 / bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: Product: (SuffixNode: PrefixNode: Literal: FloatLiteral, MultiplicationOperator, SuffixNode: PrefixNode: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAdditionAndMultiplicationOrder()
        {
            var source = @"foo = 5 + 1.2 * bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: SuffixNode: PrefixNode: Literal: IntegerLiteral, SumOperator, Product: (SuffixNode: PrefixNode: Literal: FloatLiteral, MultiplicationOperator, SuffixNode: PrefixNode: Symbol))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubtractionAndDivisionOrder()
        {
            var source = @"foo = 1.5  - bar / 15;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, Value: Sum: (Product: SuffixNode: PrefixNode: Literal: FloatLiteral, SumOperator, Product: (SuffixNode: PrefixNode: Symbol, MultiplicationOperator, SuffixNode: PrefixNode: Literal: IntegerLiteral))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
