using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexer;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class SyntaxMatcherTests
    {
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher")]
        public void TestMethod1()
        {

            var source = @"
int a;
int b;
a = b;
";
            var matcher = new SyntaxMatcher(Tokenizer.Tokenize(source));
            var result = matcher.Match();
            Assert.IsNotNull(result);
        }
    }
}
