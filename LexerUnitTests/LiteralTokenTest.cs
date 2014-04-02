using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LaborasLangCompiler.LexingTools;
using NPEG;
using System.Text;

namespace LexerUnitTests
{
    [TestClass]
    public class LiteralTokenTest
    {
        [TestMethod]
        public void TestIntegerLiteralToken()
        {            
            var source = "foo = 1;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            ExpectedTree expTree =
                new ExpectedTree("Root",
                    new ExpectedTree("Sentence",
                        new ExpectedTree("Assignment",
                            new ExpectedTree("Symbol"),
                            new ExpectedTree("Value",
                                new ExpectedTree("Sum",
                                    new ExpectedTree("Product",
                                        new ExpectedTree("Literal",
                                            new ExpectedTree("IntegerLiteral")))))),
                        new ExpectedTree("EndOfSentence")));
            expTree.AsertEqual(tree);
        }

        [TestMethod]
        public void TestStringLiteralToken()
        {
            var source = @"foo = 'bar';";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            ExpectedTree expTree =
                new ExpectedTree("Root",
                    new ExpectedTree("Sentence",
                        new ExpectedTree("Assignment",
                            new ExpectedTree("Symbol"),
                            new ExpectedTree("Value",
                                  new ExpectedTree("Sum",
                                    new ExpectedTree("Product",
                                        new ExpectedTree("Literal",
                                            new ExpectedTree("StringLiteral")))))),
                        new ExpectedTree("EndOfSentence")));
            expTree.AsertEqual(tree);
        }

        [TestMethod]
        public void TestFloatLiteralToken()
        {
            var source = "foo = 1.1;";
            AstNode tree = Lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            ExpectedTree expTree =
                new ExpectedTree("Root",
                    new ExpectedTree("Sentence",
                        new ExpectedTree("Assignment",
                            new ExpectedTree("Symbol"),
                            new ExpectedTree("Value",
                                  new ExpectedTree("Sum",
                                    new ExpectedTree("Product",
                                        new ExpectedTree("Literal",
                                            new ExpectedTree("FloatLiteral")))))),
                        new ExpectedTree("EndOfSentence")));
            expTree.AsertEqual(tree);
        }
    }
}
