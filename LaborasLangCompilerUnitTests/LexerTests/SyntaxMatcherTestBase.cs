#define REMATCH

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
            Lexer.Lexer.WithTree(source, tree =>
            {
#if REMATCH
                System.IO.File.WriteAllText(serializedTree, tree.ToString());
#else
                string expected = ReadFromFile(serializedTree);
                Assert.AreEqual(expected, tree.ToString());
#endif
            });
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

        protected void AssertContainsNoUnknowns(string source)
        {
            Lexer.Lexer.WithTree(source, tree =>
            {
                if (ContaintsUnknowns(tree))
                {
                    Assert.Fail("Unknown symbols found in tree:\r\n" + tree.ToString());
                }
            });
        }

        protected void AssertContainsUnkowns(string source)
        {
            Lexer.Lexer.WithTree(source, tree =>
            {
                if (!ContaintsUnknowns(tree))
                {
                    Assert.Fail("Unknowns expected in tree:\r\n" + tree.ToString());
                }
            });
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
    }
}
