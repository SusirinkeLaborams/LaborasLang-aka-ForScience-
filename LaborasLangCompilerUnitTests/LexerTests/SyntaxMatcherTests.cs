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
        private const string Path = @"..\..\LexerTests\Tokens\";
        private const bool Tokenize = true;
        private const bool Rematch = true;
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher")]
        public void TestMethod1()
        {

            var source = @"
int a;
int b;
a = b;
";
            ExecuteTest(source);
        }




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
            AssertEquals(tree, actualTree);
        }

        private void AssertEquals(AstNode expected, AstNode actual)
        {
            Assert.AreEqual(expected.Type, actual.Type);

            Assert.AreEqual(expected.Content, actual.Content);

            var childs = expected.Childs.Zip(actual.Childs, (actualValue, expectedValue) => new { Actual = actualValue, Expected = expectedValue });
            foreach (var child in childs)
            {
                AssertEquals(child.Expected, child.Actual);
            }
        }
    }
}
