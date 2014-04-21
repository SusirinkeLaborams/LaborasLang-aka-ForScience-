using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompilerUnitTests.ILTests;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class FunctionTests : TestBase
    {
        [TestMethod]
        public void DeclareFunctionTest()
        {
            var source = @"int(int, bool) foo;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Declaration: (FunctionType: Type: (Symbol, FunctionTypeArgs: (Type: Symbol, Type: Symbol)), Symbol), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_NoArguments()
        {
            var source = @"foo = int() { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Function: (NamedFunctionType: Type: (Symbol, FunctionTypeArgs), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_OneArgument()
        {
            var source = @"foo = int(int a) { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Function: (NamedFunctionType: (Type: Symbol, FunctionArgumentDeclaration: (Type: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_TwoArguments()
        {
            var source = @"foo = int(int a, float c) { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Function: (NamedFunctionType: (Type: Symbol, FunctionArgumentDeclaration: (Type: Symbol, Symbol), FunctionArgumentDeclaration: (Type: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_MultipleArguments()
        {
            var source = @"foo = int(int a, float c, float d,  float d2) { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (Symbol, AssignmentOperator, Function: (NamedFunctionType: (Type: Symbol, FunctionArgumentDeclaration: (Type: Symbol, Symbol), FunctionArgumentDeclaration: (Type: Symbol, Symbol), FunctionArgumentDeclaration: (Type: Symbol, Symbol), FunctionArgumentDeclaration: (Type: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HelloWorldTest()
        {
            var source = @"
                auto Main = int()
                {
                    System.Console.WriteLine('Hello, world');
                    System.Console.ReadKey();
                    return 0;
                };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (DeclarationAndAssignment: (FunctionType: Type: Symbol, Symbol, Function: (NamedFunctionType: Type: (Symbol, FunctionTypeArgs), CodeBlock: (Sentence: (FunctionCall: (FullSymbol: (Symbol, Symbol, Symbol), FunctionArgument: Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: StringLiteral), EndOfSentence), Sentence: (FunctionCall: FullSymbol: (Symbol, Symbol, Symbol), EndOfSentence), Sentence: (ReturnSentence: Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral, EndOfSentence)))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
