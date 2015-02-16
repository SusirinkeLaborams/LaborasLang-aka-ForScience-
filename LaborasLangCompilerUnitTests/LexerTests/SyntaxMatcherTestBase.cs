//#define REMATCH

using Lexer;
using Lexer.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    public class SyntaxMatcherTestBase
    {
        private const string Path = @"..\..\LexerTests\Tokens\";
        
        protected void ExecuteTest(string source, [CallerMemberName] string fileName = "")
        {
            var tokenizedSource = Path + fileName + "_tokens.txt";
            var serializedTree = Path + fileName + "_tree.txt";

            using (var rootNode = new RootNode())
            {
                var tokens = Tokenizer.Tokenize(source, rootNode);

                string tree = null;

#if REMATCH
                tree = new SyntaxMatcher(tokens, rootNode).Match().ToString();
                System.IO.File.WriteAllText(serializedTree, tree);
#else
                try
                {
                    tree = System.IO.File.ReadAllText(serializedTree);
                }
                catch
                {
                }
#endif

                var syntaxMatcher = new SyntaxMatcher(tokens, rootNode);
                var actualTree = syntaxMatcher.Match();

                var actualTreeString = actualTree.ToString();
                Assert.AreEqual(tree, actualTreeString);
            }
        }

        private void AssertNoUnknowns(AstNode tree)
        {
            Assert.AreNotEqual(TokenType.UnknownNode, tree.Token.Type);

            foreach (var child in tree.Children)
            {
                AssertNoUnknowns(child);
            }
        }

        protected void AssertCanBeLexed(string source)
        {
            using (var rootNode = new RootNode())
            {
                var tokens = Tokenizer.Tokenize(source, rootNode);
                var syntaxMatcher = new SyntaxMatcher(tokens, rootNode);
                var tree = syntaxMatcher.Match();
                AssertNoUnknowns(tree);
            }
        }
    }
}
