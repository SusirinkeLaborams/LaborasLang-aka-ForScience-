using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexer;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Text;
using System.IO;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class SyntaxMatcherTests
    {
        private const int timeout = 0;
        private const string Path = @"..\..\LexerTests\Tokens\";
        private const bool Tokenize = true;
        private const bool Rematch = true;
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMethod1()
        {

            var source = @"
int a;
int b;
a = b;
";
            ExecuteTest(source);
        }


        #region tests
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareFunctionTest()
        {
            var source = @"int(int, bool) foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void FunctionCall()
        {
            var source = @"foo();";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareFunctionTestNoParameters()
        {
            var source = @"int() foo;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareAndAssignFunction()
        {
            var source = @"auto foo = int() {foo();};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareAndAssignFunctionWithParameter()
        {
            var source = @"auto foo = int(param p) {foo();};";
            ExecuteTest(source);
        }
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareVariable()
        {
            var source = @"int foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanAnd()
        {
            var source = @"auto i = i && false;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanOr()
        {
            var source = @"auto a = i || true;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_NoArguments()
        {
            var source = @"foo = int() { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_OneArgument()
        {
            var source = @"foo = int(int a) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_TwoArguments()
        {
            var source = @"foo = int(int a, float c) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_MultipleArguments()
        {
            var source = @"foo = int(int a, float c, float d,  float d2) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void HelloWorldTest()
        {
            var source = @"
            auto Main = void()
            {
                System.Console.WriteLine('Hello, world');
                System.Console.ReadKey();
                return 0;
            };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestIntegerLiteralToken()
        {
            var source = "foo = 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestNegativeIntegerToken()
        {
            var source = "foo = -1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestStringLiteralToken()
        {
            var source = @"foo = 'bar';";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanLiteralTrue()
        {
            var source = @"foo = true;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanLiteralFalse()
        {
            var source = @"foo = false;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestStringLiteralTokenDoubleQuote()
        {
            var source = @"foo = ""bar"";";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestFloatLiteralToken()
        {
            var source = "foo = 1.1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestWhileLoop()
        {
            string source = "while(condition){function();}";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestUnaryNegation()
        {
            var source = "foo = -bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestAddition()
        {
            var source = @"foo = 1 + bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBinaryAnd()
        {
            var source = @"foo = 1 & bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBinaryOr()
        {
            var source = @"foo = 1 | bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSubtraction()
        {
            var source = @"foo = bar - 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMultiplication()
        {
            var source = @"foo = bar * 1.5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestDivision()
        {
            var source = @"foo = 1.5 / bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestAdditionAndMultiplicationOrder()
        {
            var source = @"foo = 5 + 1.2 * bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSubtractionAndDivisionOrder()
        {
            var source = @"foo = 1.5  - bar / 15;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSinglePrefix()
        {
            var source = "foo = ++i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestDoublePrefix()
        {
            var source = "foo = ++ ++i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMultiplePrefixes()
        {
            var source = "foo = --++~!++--i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSingleSuffix()
        {
            var source = "foo = i++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestDoubleSuffix()
        {
            var source = "foo = i++ ++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMultipleSuffixes()
        {
            var source = "foo = i++--++--++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestPrefixSuffix()
        {
            var source = "foo = ++i++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSuffixPrefixSum()
        {
            var source = "foo = i++ + ++i;";
            ExecuteTest(source);
        }
        #endregion tests



        private void ExecuteTest(string source, [CallerMemberName] string fileName = "")
        {
            var tokenizedSource = Path + fileName + "_tokens.xml";
            var serializedTree = Path + fileName + "_tree.xml";
            IEnumerable<Token> tokens = null;
            AstNode tree = null;
            if (Tokenize)
            {
                tokens = Tokenizer.Tokenize(source);
                var serializer = new DataContractSerializer(typeof(IEnumerable<Token>));
                using (var writer = new XmlTextWriter(tokenizedSource, Encoding.UTF8))
                {
                    serializer.WriteObject(writer, tokens);
                }
            }
            else
            {
                var serializer = new DataContractSerializer(typeof(IEnumerable<Token>));
                using (var streamReader = new StreamReader(fileName))
                {
                    using (var reader = new XmlTextReader(streamReader))
                    {
                        tokens = (IEnumerable<Token>)serializer.ReadObject(reader);
                    }
                }
            }
            
            if(Rematch)
            {
                var matcher = new SyntaxMatcher(tokens);
                tree = matcher.Match();
                var serializer = new DataContractSerializer(typeof(AstNode));
                using (var writer = new XmlTextWriter(serializedTree, Encoding.UTF8))
                {
                    serializer.WriteObject(writer, tree);
                }
            }
            else
            {
                var serializer = new DataContractSerializer(typeof(AstNode));
                using (var streamReader = new StreamReader(serializedTree))
                {
                    using (var reader = new XmlTextReader(streamReader))
                    {
                        tree = (AstNode)serializer.ReadObject(reader);
                    }                    
                }
            }

            Assert.IsNotNull(tree);
            var syntaxMatcher = new SyntaxMatcher(tokens);

            AstNode actualTree = syntaxMatcher.Match();
            Assert.IsNotNull(actualTree);
            AssertEqual(tree, actualTree);
        }

        private void AssertEqual(AstNode expected, AstNode actual)
        {
            Assert.AreEqual(expected.Type, actual.Type);

            Assert.IsTrue(expected.Content == actual.Content);

            var childs = expected.Children.Zip(actual.Children, (actualValue, expectedValue) => new { Actual = actualValue, Expected = expectedValue });
            foreach (var child in childs)
            {
                AssertEqual(child.Expected, child.Actual);
            }
        }
    }
}
