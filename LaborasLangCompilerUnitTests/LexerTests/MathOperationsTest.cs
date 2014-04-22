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
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: (PrefixOperator, FullSymbol: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAddition()
        {
            var source = @"foo = 1 + bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: (Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral, SumOperator, Product: BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBinaryAnd()
        {
            var source = @"foo = 1 & bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: (SuffixNode: PrefixNode: Literal: IntegerLiteral, BinaryOperator, SuffixNode: PrefixNode: FullSymbol: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBinaryOr()
        {
            var source = @"foo = 1 | bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: (SuffixNode: PrefixNode: Literal: IntegerLiteral, BinaryOperator, SuffixNode: PrefixNode: FullSymbol: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubtraction()
        {
            var source = @"foo = bar - 1;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: (Product: BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol, SumOperator, Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultiplication()
        {
            var source = @"foo = bar * 1.5;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: (BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol, MultiplicationOperator, BinaryOperationNode: SuffixNode: PrefixNode: Literal: FloatLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDivision()
        {
            var source = @"foo = 1.5 / bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: (BinaryOperationNode: SuffixNode: PrefixNode: Literal: FloatLiteral, MultiplicationOperator, BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAdditionAndMultiplicationOrder()
        {
            var source = @"foo = 5 + 1.2 * bar;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: (Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral, SumOperator, Product: (BinaryOperationNode: SuffixNode: PrefixNode: Literal: FloatLiteral, MultiplicationOperator, BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSubtractionAndDivisionOrder()
        {
            var source = @"foo = 1.5  - bar / 15;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: (Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: FloatLiteral, SumOperator, Product: (BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol, MultiplicationOperator, BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
