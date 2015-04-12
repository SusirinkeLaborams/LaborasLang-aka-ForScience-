using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class ValueBlockTests : SyntaxMatcherTestBase
    {
        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestBasicOperator()
        {
            var source = "a + b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("ab+;", actual.FullContent);
        }

        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestSamePrecedenceOperators()
        {
            var source = "a + b - c + d;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("ab+c-d+;", actual.FullContent);
        }

        struct TestCase {
            public string Source;
            public string Expected;
        }

        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestDifferentPrecedenceOperators()
        {
            var sources = new[] {
                new TestCase(){Source="a + b * c;", Expected = "abc*+;"},
                new TestCase(){Source="a * b + c;", Expected = "ab*c+;"},
                new TestCase(){Source="a / b * c;", Expected = "ab/c*;"}};

            var actual = sources.Select(s => Lexer.Lexer.Lex(s.Source).FullContent).ToList();
            var expected = sources.Select(s => s.Expected).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }


        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestAssignmentOperator()
        {
            var sources = new[] {
                new TestCase(){Source="a = b;", Expected = "ab=;"},
                new TestCase(){Source="a = b + c;", Expected = "abc+=;"},
                new TestCase(){Source="d = a / b * c;", Expected = "dab/c*=;"},};

            IEnumerable<string> actual = sources.Select(s => Lexer.Lexer.Lex(s.Source).FullContent);
            IEnumerable<string> expected = sources.Select(s => s.Expected);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }


        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestPrefixes()
        {
            var sources = new[] {
                new TestCase(){Source="++i;", Expected = "i++;"},
                new TestCase(){Source="a = ++i;" ,Expected = "ai++=;"},
                new TestCase(){Source="a = ++b * c;", Expected = "ab++c*=;"},};

            var actual = sources.Select(s => Lexer.Lexer.Lex(s.Source).FullContent).ToList();
            var expected = sources.Select(s => s.Expected).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestPostfixes()
        {
            var sources = new[] {
                new TestCase(){Source="i++;", Expected = "i++;"},
                new TestCase(){Source="a = i++;" ,Expected = "ai++=;"},
                new TestCase(){Source="a = b++ * c;", Expected = "ab++c*=;"},};

            var actual = sources.Select(s => Lexer.Lexer.Lex(s.Source).FullContent).ToList();
            var expected = sources.Select(s => s.Expected).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }


        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestPostfixPrefixMix()
        {
            var sources = new[] {
                new TestCase(){Source="++i++;", Expected = "i++++;"},
                new TestCase(){Source="a = ++i++;" ,Expected = "ai++++=;"},
                new TestCase(){Source="a = ++b++ * c;", Expected = "ab++++c*=;"},};

            var actual = sources.Select(s => Lexer.Lexer.Lex(s.Source).FullContent).ToList();
            var expected = sources.Select(s => s.Expected).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
