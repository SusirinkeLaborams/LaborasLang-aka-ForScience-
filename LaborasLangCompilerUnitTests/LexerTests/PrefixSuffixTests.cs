using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompilerUnitTests.ILTests;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class PrefixSuffixTests : TestBase
    {
        [TestMethod]
        public void TestSinglePrefix()
        {
            var source = "foo = ++i;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: (PrefixOperator, Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDoublePrefix()
        {
            var source = "foo = ++ ++i;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: (PrefixOperator, PrefixOperator, Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultiplePrefixes()
        {
            var source = "foo = --++~!++--i;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: (PrefixOperator, PrefixOperator, PrefixOperator, PrefixOperator, PrefixOperator, PrefixOperator, Symbol)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSingleSuffix()
        {
            var source = "foo = i++;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: (PrefixNode: Symbol, SuffixOperator)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDoubleSuffix()
        {
            var source = "foo = i++ ++;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: (PrefixNode: Symbol, SuffixOperator, SuffixOperator)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultipleSuffixes()
        {
            var source = "foo = i++--++--++;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: (PrefixNode: Symbol, SuffixOperator, SuffixOperator, SuffixOperator, SuffixOperator, SuffixOperator)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPrefixSuffix()
        {
            var source = "foo = ++i++;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: (PrefixNode: (PrefixOperator, Symbol), SuffixOperator)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSuffixPrefixSum()
        {
            var source = "foo = i++ + ++i;";
            AstNode tree = lexer.MakeTree(source);
            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Value: Comparison: BooleanNode: Sum: (Product: BinaryOperationNode: SuffixNode: (PrefixNode: Symbol, SuffixOperator), SumOperator, Product: BinaryOperationNode: SuffixNode: PrefixNode: (PrefixOperator, Symbol))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
