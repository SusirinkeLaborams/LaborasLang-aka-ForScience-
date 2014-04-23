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
            string expected = "Root: Sentence: (Declaration: (FunctionType: Type: (FullSymbol: Symbol, FunctionTypeArgs: (Type: FullSymbol: Symbol, Type: FullSymbol: Symbol)), Symbol), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeclareFunctionTestNoParameters()
        {
            var source = @"int() foo;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Declaration: (FunctionType: Type: (FullSymbol: Symbol, FunctionTypeArgs), Symbol), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void DeclareAndAssignFunction()
        {
            var source = @"auto foo = int() {foo();};";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (DeclarationAndAssignment: (FunctionType: Type: FullSymbol: Symbol, Symbol, Function: (NamedFunctionType: Type: FullSymbol: Symbol, CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DeclareAndAssignFunctionWithParameter()
        {
            var source = @"auto foo = int(param p) {foo();};";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (DeclarationAndAssignment: (FunctionType: Type: FullSymbol: Symbol, Symbol, Function: (NamedFunctionType: (Type: FullSymbol: Symbol, FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void DeclareVariable()
        {
            var source = @"int foo;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Declaration: (FunctionType: Type: FullSymbol: Symbol, Symbol), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBooleanAnd()
        {
            var source = @"auto i = i && false;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (DeclarationAndAssignment: (FunctionType: Type: FullSymbol: Symbol, Symbol, Value: Comparison: BooleanNode: (Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol, BooleanOperator, Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: BooleanLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBooleanOr()
        {
            var source = @"auto a = i || true;";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (DeclarationAndAssignment: (FunctionType: Type: FullSymbol: Symbol, Symbol, Value: Comparison: BooleanNode: (Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: FullSymbol: Symbol, BooleanOperator, Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: BooleanLiteral)), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void AssignFunctionTest_NoArguments()
        {
            var source = @"foo = int() { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Function: (NamedFunctionType: Type: FullSymbol: Symbol, CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_OneArgument()
        {
            var source = @"foo = int(int a) { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Function: (NamedFunctionType: (Type: FullSymbol: Symbol, FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_TwoArguments()
        {
            var source = @"foo = int(int a, float c) { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Function: (NamedFunctionType: (Type: FullSymbol: Symbol, FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol), FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssignFunctionTest_MultipleArguments()
        {
            var source = @"foo = int(int a, float c, float d,  float d2) { bar(); };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (Assignment: (FullSymbol: Symbol, AssignmentOperator, Function: (NamedFunctionType: (Type: FullSymbol: Symbol, FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol), FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol), FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol), FunctionArgumentDeclaration: (Type: FullSymbol: Symbol, Symbol)), CodeBlock: Sentence: (FunctionCall: FullSymbol: Symbol, EndOfSentence))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HelloWorldTest()
        {
            var source = @"
                auto Main = void()
                {
                    System.Console.WriteLine('Hello, world');
                    System.Console.ReadKey();
                    return 0;
                };";
            AstNode tree = lexer.MakeTree(source);

            Assert.IsNotNull(tree);
            string expected = "Root: Sentence: (DeclarationAndAssignment: (FunctionType: Type: FullSymbol: Symbol, Symbol, Function: (NamedFunctionType: Type: FullSymbol: Symbol, CodeBlock: (Sentence: (FunctionCall: (FullSymbol: (Symbol, Symbol, Symbol), FunctionArgument: Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: StringLiteral), EndOfSentence), Sentence: (FunctionCall: FullSymbol: (Symbol, Symbol, Symbol), EndOfSentence), Sentence: (ReturnSentence: Value: Comparison: BooleanNode: Sum: Product: BinaryOperationNode: SuffixNode: PrefixNode: Literal: IntegerLiteral, EndOfSentence)))), EndOfSentence)";
            string actual = AstHelper.Stringify(tree);
            Assert.AreEqual(expected, actual);
        }
    }
}
