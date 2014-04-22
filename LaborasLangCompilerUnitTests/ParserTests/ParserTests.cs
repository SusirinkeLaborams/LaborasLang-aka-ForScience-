using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompilerUnitTests.ILTests;
using LaborasLangCompilerUnitTests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPEG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    [TestClass]
    public class ParserTests : TestBase
    {
        private const string path = @"..\..\ParserTests\SerializedLexerTrees\";
        private const bool lex = false;
        [TestMethod, TestCategory("Parser")]
        public void FieldDeclarationTest()
        {
            string source = "auto a = 5; int b; int c = 10;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b, System.Int32 c = (Literal: System.Int32 10))";
            TestParser(source, expected, "FieldDeclarationTest", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void ImplicitIntToLong()
        {
            string source = "auto a = 5; long b = a;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int64 b = (LValueNode: Field System.Int32))";
            TestParser(source, expected, "ImplicitIntToLong", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TypeExceptionTest()
        {
            string source = "int a = 0.0;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Single 0))";
            try
            {
                TestParser(source, expected, "TypeExceptionTest", lex);
            }
            catch(TypeException)
            {
                //should throw
                return;
            }
            //no type error
            Assert.Fail();
        }
        [TestMethod, TestCategory("Parser")]
        public void MethodCallTest()
        {
            string source = @"
                auto Main = void(int arg)
                {
	                Main(4);
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void$System_Int32 Main = (Function: $Functors.$System_Void$System_Int32(System.Int32 arg)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) arg) Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.Int32 4) Function: (LValueNode: Field $Functors.$System_Void$System_Int32))))))";
            TestParser(source, expected, "MethodCallTest", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void StringLiteralTest()
        {
            string source = @"auto a = ""word"";";
            string expected = "(ClassNode: Fields: System.String a = (Literal: System.String word))";
            TestParser(source, expected, "StringLiteralTest", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void HelloWorld()
        {
            string source = @"
                auto Main = void()
                {
	                System.Console.WriteLine(""Hello, World!"");
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (Function: $Functors.$System_Void()(CodeBlock: Symbols: () Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.String Hello, World!) Function: (Method: System.Void System.Console::WriteLine(System.String)))))))";
            TestParser(source, expected, "HelloWorld", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void While()
        {
            string source = @"
                auto Main = void()
                {
                    bool a;
                    int c = 5;
	                while(a)
                    {
                        c += 1;
                    }
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (Function: $Functors.$System_Void()(CodeBlock: Symbols: ((LValueNode: LocalVariable System.Boolean) a, (LValueNode: LocalVariable System.Int32) c) Nodes: ((Declaration: (LValueNode: LocalVariable System.Boolean) = ), (Declaration: (LValueNode: LocalVariable System.Int32) = (Literal: System.Int32 5)), (WhileBlock: Condition: (LValueNode: LocalVariable System.Boolean), Block: (CodeBlock: Symbols: () Nodes: ((UnaryOp: VoidOperator (Assignment: (LValueNode: LocalVariable System.Int32) = (BinaryOp: (LValueNode: LocalVariable System.Int32) Addition (Literal: System.Int32 1))))))))))";
            TestParser(source, expected, "While", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void If()
        {
            string source = @"
                auto Main = void()
                {
                    if(true)
                    {
                        int a;
                    }
                    else
                    {
                        int b;
                    }
                    if(false)
                    {
                        int c;
                    }
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (Function: $Functors.$System_Void()(CodeBlock: Symbols: () Nodes: ((ConditionBlock: Condition: (Literal: System.Boolean True), True: (CodeBlock: Symbols: ((LValueNode: LocalVariable System.Int32) a) Nodes: ((Declaration: (LValueNode: LocalVariable System.Int32) = ))), False: (CodeBlock: Symbols: ((LValueNode: LocalVariable System.Int32) b) Nodes: ((Declaration: (LValueNode: LocalVariable System.Int32) = ))), (ConditionBlock: Condition: (Literal: System.Boolean False), True: (CodeBlock: Symbols: ((LValueNode: LocalVariable System.Int32) c) Nodes: ((Declaration: (LValueNode: LocalVariable System.Int32) = ))), False: ))))";
            TestParser(source, expected, "If", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void SomeTest()
        {
            string source = @"
                int a = 5;
                int b = a;
                auto Main = void(int arg)
                {
	                int a = 20;
	                a = 10;

	                auto f = int(int a, float b)
	                {
		                auto c = a * b;
                        return a;
	                };
                };";
            string expected = @"(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b = (LValueNode: Field System.Int32), $Functors.$System_Void$System_Int32 Main = (Function: $Functors.$System_Void$System_Int32(System.Int32 arg)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) arg, (LValueNode: LocalVariable System.Int32) a, (LValueNode: LocalVariable $Functors.$System_Int32$System_Int32$System_Single) f) Nodes: ((Declaration: (LValueNode: LocalVariable System.Int32) = (Literal: System.Int32 20)), (UnaryOp: VoidOperator (Assignment: (LValueNode: LocalVariable System.Int32) = (Literal: System.Int32 10))), (Declaration: (LValueNode: LocalVariable $Functors.$System_Int32$System_Int32$System_Single) = (Function: $Functors.$System_Int32$System_Int32$System_Single(System.Int32 a, System.Single b)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) a, (LValueNode: FunctionArgument System.Single) b, (LValueNode: LocalVariable System.Single) c) Nodes: ((Declaration: (LValueNode: LocalVariable System.Single) = (BinaryOp: (LValueNode: FunctionArgument System.Int32) Multiplication (LValueNode: FunctionArgument System.Single))), (ReturnNode: (LValueNode: FunctionArgument System.Int32))))))))))";
            TestParser(source, expected, "SomeTest", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void AssignmentArithmeticOperatorTest()
        {
            string source = @"
                auto a = 5;
                auto Main = void(int b)
                {
                    a += b;
                    b *= a;
                    a /= 5;
                    a -= 8;
                };";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), $Functors.$System_Void$System_Int32 Main = (Function: $Functors.$System_Void$System_Int32(System.Int32 b)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) b) Nodes: ((UnaryOp: VoidOperator (Assignment: (LValueNode: Field System.Int32) = (BinaryOp: (LValueNode: Field System.Int32) Addition (LValueNode: FunctionArgument System.Int32)))), (UnaryOp: VoidOperator (Assignment: (LValueNode: FunctionArgument System.Int32) = (BinaryOp: (LValueNode: FunctionArgument System.Int32) Multiplication (LValueNode: Field System.Int32)))), (UnaryOp: VoidOperator (Assignment: (LValueNode: Field System.Int32) = (BinaryOp: (LValueNode: Field System.Int32) Division (Literal: System.Int32 5)))), (UnaryOp: VoidOperator (Assignment: (LValueNode: Field System.Int32) = (BinaryOp: (LValueNode: Field System.Int32) Subtraction (Literal: System.Int32 8))))))))";
            TestParser(source, expected, "AssignmentArithmeticOperatorTest", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestPrecedence()
        {
            string source = "auto a = 5 * 4 + 8 * (2 + 1);";
            string expected = "(ClassNode: Fields: System.Int32 a = (BinaryOp: (BinaryOp: (Literal: System.Int32 5) Multiplication (Literal: System.Int32 4)) Addition (BinaryOp: (Literal: System.Int32 8) Multiplication (BinaryOp: (Literal: System.Int32 2) Addition (Literal: System.Int32 1)))))";
            TestParser(source, expected, "TestPrecedence", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnValue()
        {
            string source = @"auto Main = int(int b)
                {
                    return b;
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32$System_Int32 Main = (Function: $Functors.$System_Int32$System_Int32(System.Int32 b)(CodeBlock: Symbols: ((LValueNode: FunctionArgument System.Int32) b) Nodes: ((ReturnNode: (LValueNode: FunctionArgument System.Int32))))))";
            TestParser(source, expected, "TestReturnValue", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUnaryOrder()
        {
            string source = @"
                auto i = 1;
                auto a = ++--i;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PreIncrement (UnaryOp: PreDecrement (LValueNode: Field System.Int32))))";
            TestParser(source, expected, "TestUnaryOrder", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSuffixOrder()
        {
            string source = @"
                auto i = 1;
                auto a = i++--;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PostDecrement (UnaryOp: PostIncrement (LValueNode: Field System.Int32))))";
            TestParser(source, expected, "TestSuffixOrder", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void HarderBetterFasterStronger()
        {
            string source = @"
                auto i = 1;
                auto a = ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                         i
                         ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (LValueNode: Field System.Int32))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))";
            TestParser(source, expected, "HarderBetterFasterStronger", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMixedSuffixPrefix()
        {
            string source = @"
                auto i = 1;
                auto a = -++i--;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PostDecrement (UnaryOp: Negation (UnaryOp: PreIncrement (LValueNode: Field System.Int32)))))";
            TestParser(source, expected, "TestMixedSuffixPrefix", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestStringPlusNumber()
        {
            string source = @"
                auto a = 5 + ""something"";";
            string expected = "(ClassNode: Fields: System.String a = (BinaryOp: (Literal: System.Int32 5) Addition (Literal: System.String something)))";
            TestParser(source, expected, "TestStringPlusNumber", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestBinaryOps()
        {
            string source = @"
                auto i = 1 ^ 2;
                auto a = i & i;
                int c = i | a;";
            string expected = "(ClassNode: Fields: System.Int32 i = (BinaryOp: (Literal: System.Int32 1) BinaryXor (Literal: System.Int32 2)), System.Int32 a = (BinaryOp: (LValueNode: Field System.Int32) BinaryAnd (LValueNode: Field System.Int32)), System.Int32 c = (BinaryOp: (LValueNode: Field System.Int32) BinaryOr (LValueNode: Field System.Int32)))";
            TestParser(source, expected, "TestBinaryOps", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestLogicalOps()
        {
            string source = @"
                auto i = true && false;
                auto a = i || true;";
            string expected = "";
            TestParser(source, expected, "TestLogicalOps", true);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestComparison()
        {
            string source = @"
                auto i = true == false;
                auto a = i != true;
                auto c = 5 > 6;
                auto b = 8 <= 10;";
            string expected = "(ClassNode: Fields: System.Boolean i = (BinaryOp: (Literal: System.Boolean True) Equals (Literal: System.Boolean False)), System.Boolean a = (BinaryOp: (LValueNode: Field System.Boolean) NotEquals (Literal: System.Boolean True)), System.Boolean c = (BinaryOp: (Literal: System.Int32 5) GreaterThan (Literal: System.Int32 6)), System.Boolean b = (BinaryOp: (Literal: System.Int32 8) LessEqualThan (Literal: System.Int32 10)))";
            TestParser(source, expected, "TestComparison", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnTypeFailure()
        {
            string source = @"
                auto Main = int(){return 4.0;};";
            string expected = "";
            try
            {
                TestParser(source, expected, "TestReturnTypeFailure", lex);
            }
            catch(TypeException)
            {
                return;
            }
            Assert.Fail("Tried to return float instead of int, should've failed");
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnTypeSuccess()
        {
            string source = @"
                auto Main = int(){return 4;};";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (Function: $Functors.$System_Int32()(CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 4))))))";
            TestParser(source, expected, "TestReturnTypeSuccess", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestEnforceReturn1()
        {
            string source = @"
                auto Main = int()
                {
                    if(true)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (Function: $Functors.$System_Int32()(CodeBlock: Symbols: () Nodes: ((ConditionBlock: Condition: (Literal: System.Boolean True), True: (CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 1)))), False: (CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 0))))))))";
            TestParser(source, expected, "TestEnforceReturn1", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestEnforceReturn2()
        {
            string source = @"
                auto Main = int()
                {
                    if(true)
                    {
                        return 1;
                    }
                };";
            string expected = "";
            try
            {
                TestParser(source, expected, "TestEnforceReturn2", lex);
            }
            catch(ParseException)
            {
                return;
            }
            Assert.Fail("Not all paths return, should've failed");
        }
        [TestMethod, TestCategory("Parser")]
        public void TestEnforceReturn3()
        {
            string source = @"
                auto Main = int()
                {
                    if(true)
                    {
                        return 1;
                    }
                    return 0;
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (Function: $Functors.$System_Int32()(CodeBlock: Symbols: () Nodes: ((ConditionBlock: Condition: (Literal: System.Boolean True), True: (CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 1)))), False: , (ReturnNode: (Literal: System.Int32 0))))))";
            TestParser(source, expected, "TestEnforceReturn3", lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestEnforceReturn4()
        {
            string source = @"
                auto Main = int()
                {
                    {
                        return 500;
                    }
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (Function: $Functors.$System_Int32()(CodeBlock: Symbols: () Nodes: ((CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 500))))))))";
            TestParser(source, expected, "TestEnforceReturn4", lex);
        }
        private void TestParser(string source, string expected, string name, bool lex)
        {
            var compilerArgs = CompilerArguments.Parse(new[] { name + ".ll" });
            var assembly = new AssemblyEmitter(compilerArgs);
            var bytes = SourceReader.ReadSource(source);
            AstNode tree;
            if(lex)
            {
                tree = lexer.MakeTree(bytes);
                TreeSerializer.Serialize(path + name + ".xml", tree);
            }
            else
            {
                tree = TreeSerializer.Deserialize(path + name + ".xml");
            }
            Parser parser = new Parser(assembly, tree, bytes, "test", true);
            string result = parser.Root.ToString();
            Assert.AreEqual(expected, result);
        }
    }
}
