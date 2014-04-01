using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LaborasLangCompiler.LexingTools;
using NPEG;

namespace LexerUnitTests
{
    [TestClass]
    public class LiteralTest
    {
        [TestMethod]
        public void TestIntegerLiteral()
        {
            
            var source = @"foo = 1;";
            byte[] bytes = new byte[source.Length * sizeof(char)];
            System.Buffer.BlockCopy(source.ToCharArray(), 0, bytes, 0, bytes.Length);
            ByteInputIterator inputIterator = new ByteInputIterator(bytes);


            AstNode tree = Lexer.MakeTree(inputIterator);

            Assert.IsNotNull(tree);
            ExpectedTree expTree =
                new ExpectedTree("Root",
                    new ExpectedTree("Sentence",
                        new ExpectedTree("Assignment",
                            new ExpectedTree("Symbol"),
                            new ExpectedTree("Value",
                                new ExpectedTree("Literal",
                                    new ExpectedTree("StringLiteral")))),
                        new ExpectedTree("EndOfSentence")));
            expTree.AsertEqual(tree);
        }
    }
}
