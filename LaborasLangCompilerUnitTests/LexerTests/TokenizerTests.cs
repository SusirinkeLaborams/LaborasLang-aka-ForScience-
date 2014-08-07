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
        private const bool Tokenize = false;
        private RootNode rootNode = new RootNode();

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizer()
        {
            var source =
@"auto Main = void()
{
	System.Console.WriteLine('Hello, world!');
	System.Console.ReadKey();
};";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }
        #region Single branch tests
        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerStringLiteral1()
        {
            var source = @"'str\'ing'";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerStringLiteral2()
        {
            var source = @"""str\""ing""";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerPlus()
        {
            var source = @"+ += ++";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerMinus()
        {
            var source = @"- -= --";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerNot()
        {
            var source = @"! !=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerBitwiseComplement()
        {
            var source = @"~ ~=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerAnd()
        {
            var source = @"& && &=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerXor()
        {
            var source = @"^ ^=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerOr()
        {
            var source = @"| |= ||";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerLessThan()
        {
            var source = @"< << <= <<=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerMoreThan()
        {
            var source = @"> >> >= >>=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerDivision()
        {
            var source = @"/ /= /* content */ //content
";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerMultiplication()
        {
            var source = @"* *=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerRemainder()
        {
            var source = @"% %=";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerEqual()
        {
            var source = @"= ==";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerCurlyBracket()
        {
            var source = @"{ }";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerBracket()
        {
            var source = @"( )";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerSemicolon()
        {
            var source = @";";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerNumber()
        {
            var source =
@"1 2 3 4 5 6 7 8 9 0 10 10f 10F 10l 10L 10.5f 10.5F 10..5 10.5L";
            var tokens = ExecuteTest(source);
            Assert.IsTrue(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerDot()
        {
            var source = @".";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Tokenizer"), Timeout(1000)]
        public void TestTokenizerSymbol()
        {
            var source = @"symbol";
            var tokens = ExecuteTest(source);
            Assert.IsFalse(ContainsMalformedTokens(tokens));
        }
        #endregion Single branch tests

        bool ContainsMalformedTokens(IEnumerable<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.MalformedToken)
                {
                    return true;
                }
            }
            return false;
        }
        IEnumerable<Token> ExecuteTest(string source, [CallerMemberName] string fileName = "")
        {
            fileName = Path + fileName + ".xml";
            Token[] tokens = null;
            if (Tokenize)
            {
                tokens = Tokenizer.Tokenize(source, rootNode);
                var serializer = new DataContractSerializer(typeof(Token[]));
                using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
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
                        tokens = (Token[])serializer.ReadObject(reader);
                    }
                }
            }
            Assert.IsNotNull(tokens);
            Token[] currentTokens = Tokenizer.Tokenize(source, rootNode);
            var pairs = currentTokens.Zip(tokens, (actual, expected) => new { Actual = actual, Expected = expected });
            foreach (var pair in pairs)
            {
                Assert.IsTrue(pair.Actual == pair.Expected);
            }
            return currentTokens;
        }

    }
}
