//﻿#define REMATCH

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

        private bool ContaintsUnknowns(AstNode tree)
        {
            if (TokenType.UnknownNode == tree.Token.Type)
            {
                return true;
            }

            foreach (var child in tree.Children)
            {
                return ContaintsUnknowns(child);
            }

            return false;
        }

        protected void AssertCanBeLexed(string source)
        {
            using (var rootNode = new RootNode())
            {
                var tokens = Tokenizer.Tokenize(source, rootNode);
                var syntaxMatcher = new SyntaxMatcher(tokens, rootNode);
                var tree = syntaxMatcher.Match();
                if(ContaintsUnknowns(tree))
                {
                    Assert.Fail("Unknown symbols found in tree:\r\n" + tree.ToString());
                }
            }
        }
    }
}
