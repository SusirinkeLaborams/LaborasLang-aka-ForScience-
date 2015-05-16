using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using Lexer;
using System.Text;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class ValueBlockTests : SyntaxMatcherTestBase
    {


        public string Structure(IAbstractSyntaxTree tree)
        {
            if (!tree.Type.IsTerminal() && tree.Children.Count == 1)
            {
                return Structure(tree.Children[0]);
            }

            var content = tree.Node.Type.IsTerminal() ? tree.Node.Content : string.Empty;
            string childrenContent = string.Empty;
            if (tree.Children.Count > 1)
            {
                childrenContent = string.Format("[{0}]", string.Join(" ", tree.Children.Select(v => Structure(v))));
            }
            else
            {
                if (tree.Children.Count == 1)
                {
                    childrenContent = Structure(tree.Children[0]);
                }
            }

            return string.Format("{0}{1}", content, childrenContent);
        }

        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestBasicOperator()
        {
            var source = "a + b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("ab+;", actual.FullContent);
        }


        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestDoubleAssignment()
        {
            var source = "a += b = c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("abc=+=;", actual.FullContent);
        }


        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestStructure()
        {
            var source = "a + b * c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [b c *] +] ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestPrefix()
        {
            var source = "a + ++b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [b ++] +] ;]", Structure(actual));
        }
        
        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestDivisionEqualityPrecedence()
        {
            var source = "value % divisor == 0;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[value divisor %] 0 ==] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedence()
        {
            var source = "ab;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[ab ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestMinusMinusPrecedence()
        {
            var source = "a - - ++b();";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [[[b [( )]] ++] -] -] ;]", Structure(actual));
        }
        
        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestComparisonDivisionEqualsPrecedence()
        {
            var source = "v % d == 0 && d < v;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[[v d %] 0 ==] [d v <] &&] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPeriodCallPrecedence()
        {
            var source = "foo().bar();";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[[foo [( )]] bar .] [( )]] ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestRightAssociativeAfterPostfix()
        {
            var source = "foo() = bar();";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[foo [( )]] [bar [( )]] =] ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePeriodAddition()
        {
            var source = "a.b + c.d;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a b .] [c d .] +] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePeriodAdditionAssignment()
        {
            var source = "foo = a.b + c.d;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[foo [[a b .] [c d .] +] =] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePrefixAssignmentAdition()
        {
            var source = "i = ++a + 1;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[i [[a ++] 1 +] =] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceShiftAssignment()
        {
            var source = "a = b << c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [b c <<] =] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceShiftAddition()
        {
            var source = "a + c << b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a c +] b <<] ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceAditionMultiplication()
        {
            var source = "a + b * c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [b c *] +] ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceMultiplicationBiwise()
        {
            var source = "a * b | c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [b c |] *] ;]", Structure(actual));
        }


        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceBitwisePrefix()
        {
            var source = "++a | b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a ++] b |] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceBitwisePrefix2()
        {
            var source = "a | ++b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [b ++] |] ;]", Structure(actual));
        }

        
        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestPrefixMultiplication()
        {
            var source = "a + ++b * c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[a [[b ++] c *] +] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePrefixPostfix()
        {
            var source = "!b--;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[b --] !] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceComparisonMultiplication()
        {
            var source = "a * b > c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a b *] c >] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedenceLogicalMultiplication()
        {
            var source = "a * b && 3;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a b *] 3 &&] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePeriodPostfix()
        {
            var source = "a.b++;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a b .] ++] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestConsoleReadLinePrecedence()
        {
            var source = "Console.ReadLine();";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[Console ReadLine .] [( )]] ;]", Structure(actual));
        }
        
        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePrefixParenthesis()
        {
            var source = "++foo();";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[foo [( )]] ++] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: value processor")]
        public void TestRightAssociativeOperators()
        {
            var source = "foo = bar = foobar = foobar2k;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[foo [bar [foobar foobar2k =] =] =] ;]", Structure(actual));
        }
        
        [TestMethod, TestCategory("Lexer: operator precedece"), TestCategory("Lexer")]
        public void TestPrecedencePeriodPrefix()
        {
            var source = "++a.b;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("[[[a b .] ++] ;]", Structure(actual));
        }

        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestSamePrecedenceOperators()
        {
            var source = "a + b - c + d;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("ab+c-d+;", actual.FullContent);
        }

        [TestMethod, TestCategory("Lexer: value processor"), TestCategory("Lexer")]
        public void TestAssignToProperty()
        {
            var source = "a.b=c;";
            var actual = Lexer.Lexer.Lex(source);

            Assert.AreEqual("ab.c=;", actual.FullContent);
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
