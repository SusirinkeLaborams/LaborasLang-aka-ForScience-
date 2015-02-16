//#define TOKENIZE

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;
using System.Linq;
using Lexer.Containers;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class TokenizerTests
    {
        private const string Path = @"..\..\LexerTests\Tokens\";

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizer()
        {
            var source =
@"auto Main = void()
{
	System.Console.WriteLine('Hello, world!');
	System.Console.ReadKey();
};";
            ExecuteTest(source);
        }

        #region Single branch tests

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerArrayIndexing()
        {
            var source = @"bar[foo]";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerStringLiteral1()
        {
            var source = @"'str\'ing'";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerStringLiteral2()
        {
            var source = @"""str\""ing""";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerPlus()
        {
            var source = @"+ += ++";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerMinus()
        {
            var source = @"- -= --";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerNot()
        {
            var source = @"! !=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerBitwiseComplement()
        {
            var source = @"~";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerAnd()
        {
            var source = @"& && &= &&=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerXor()
        {
            var source = @"^ ^=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerOr()
        {
            var source = @"| |= || ||=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerLessThan()
        {
            var source = @"< << <= <<=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerMoreThan()
        {
            var source = @"> >> >= >>=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerDivision()
        {
            var source = @"/ /= /* content */ //content
";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerMultiplication()
        {
            var source = @"* *=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerRemainder()
        {
            var source = @"% %=";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerEqual()
        {
            var source = @"= ==";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerCurlyBracket()
        {
            var source = @"{ }";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerBracket()
        {
            var source = @"( )";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerSemicolon()
        {
            var source = @";";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerNumber()
        {
            var source =
@"1 2 3 4 5 6 7 8 9 0 10 10f 10F 10l 10L 10.5f 10.5F 10 10.5L";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerDot()
        {
            var source = @".";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerSymbol()
        {
            var source = @"symbol";
            ExecuteTest(source);
        }
        #endregion Single branch tests

        void ExecuteTest(string source, [CallerMemberName] string fileName = "")
        {
            fileName = Path + fileName + ".txt";
            string tokenString = null;
            var builder = new StringBuilder();

            using (var rootNode = new RootNode())
            {
#if TOKENIZE
                var tokens = Tokenizer.Tokenize(source, rootNode);

                foreach (var token in tokens)
                {
                    builder.Append(token.ToString());
                    builder.Append(" ");
                }
                tokenString = builder.ToString();
                System.IO.File.WriteAllText(fileName, tokenString);
                builder.Clear();
#else
                try
                {
                    tokenString = System.IO.File.ReadAllText(fileName);
                }
                catch
                { 
                    // File will be not found if test was recently created.
                }
#endif

                foreach (var token in Tokenizer.Tokenize(source, rootNode))
                {
                    builder.Append(token.ToString());
                    builder.Append(" ");
                }
                Assert.AreEqual(tokenString, builder.ToString());
            }
        }
    }
}
