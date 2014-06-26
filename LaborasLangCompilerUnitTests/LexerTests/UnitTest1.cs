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

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class TokenizerTests
    {
        private const string Path = @"..\..\LexerTests\Tokens\";
        private const bool Tokenize = true;

        [TestMethod, TestCategory("Lexer")]
        public void TestTokenizer()
        {
            var source =
@"auto Main = void()
{
	System.Console.WriteLine('Hello, world!');
	System.Console.ReadKey();
};";
            AssertUnmodified(source);
        }

        void AssertUnmodified(string source, [CallerMemberName] string fileName = "")
        {
            fileName = Path + fileName + ".xml";
            IEnumerable<Token> tokens = null;
            if (Tokenize)
            {
                tokens = Tokenizer.Tokenize(source);
                var serializer = new DataContractSerializer(typeof(IEnumerable<Token>));
                using (var writer = new XmlTextWriter(fileName , Encoding.UTF8))
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
                        tokens = (IEnumerable<Token>) serializer.ReadObject(reader);
                    }
                }
            }
            Assert.IsNotNull(tokens);
            IEnumerable<Token> currentTokens = Tokenizer.Tokenize(source);
            var pairs = currentTokens.Zip(tokens, (actual, expected) => new { Actual = actual, Expected = expected });
            foreach (var pair in pairs)
            {
                Assert.IsTrue(pair.Actual == pair.Expected);
            }
        }

    }
}
        