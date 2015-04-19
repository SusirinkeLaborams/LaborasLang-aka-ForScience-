//#define REMATCH

using Lexer;
using Lexer.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    public class SyntaxMatcherTestBase
    {
        private const string Path = @"..\..\LexerTests\Tokens\";

        protected void AssertCanBeLexedAs(string source, TokenType rule)
        {
            using(RootNode rootNode = new RootNode()) {
                SyntaxMatcher matcher = new SyntaxMatcher(Tokenizer.Tokenize(source, rootNode), rootNode);
                var tree = matcher.Match(new[]{new Condition(rule, ConditionType.One)});
                AssertContainsNoUnknowns(tree);
            }
        }


        protected void ExecuteTest(string source, [CallerMemberName] string fileName = "")
        {
            Contract.Assume(!String.IsNullOrWhiteSpace(fileName));

            AssertContainsNoUnknowns(source);
            VerifyNotModified(source, fileName);
        }

        protected void ExecuteFailingTest(string source, [CallerMemberName] string fileName = "")
        {
            Contract.Assume(!String.IsNullOrWhiteSpace(fileName));

            AssertContainsUnkowns(source);
            VerifyNotModified(source, fileName);
        }


        private void VerifyNotModified(string source, string fileName)
        {
            var serializedTree = Path + fileName + "_tree.txt";
            var tree = Lexer.Lexer.Lex(source);            
#if REMATCH
            System.IO.File.WriteAllText(serializedTree, tree.ToString());
#else
            string expected = ReadFromFile(serializedTree);
            Assert.AreEqual(expected, tree.ToString());
#endif
        }

        private static string ReadFromFile(string fileName)
        {
            try
            {
                return System.IO.File.ReadAllText(fileName);
            }
            catch
            {
                return "";
            }
        }


        private void AssertContainsNoUnknowns(AstNode tree)
        {
            AssertContainsNoUnknowns(new AbstractSyntaxTree(tree));
        }


        private void AssertContainsNoUnknowns(IAbstractSyntaxTree tree)
        {
            if (ContaintsUnknowns(tree))
            {
                throw new Exception("Unknown symbols found in tree:\r\n" + tree.ToString());
            }
        }


        protected void AssertContainsNoUnknowns(string source)
        {
            var tree = Lexer.Lexer.Lex(source);
            if (ContaintsUnknowns(tree))
            {
                throw new Exception("Unknown symbols found in tree:\r\n" + tree.ToString());
            }         
        }

        protected void AssertContainsUnkowns(string source)
        {
            var tree = Lexer.Lexer.Lex(source);
            if (!ContaintsUnknowns(tree))
            {
                Assert.Fail("Unknowns expected in tree:\r\n" + tree.ToString());
            }
        }

        private bool ContaintsUnknowns(IAbstractSyntaxTree tree)
        {
            if (TokenType.UnknownNode == tree.Type)
            {
                return true;
            }

            foreach (var child in tree.Children)
            {
                if (ContaintsUnknowns(child))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
