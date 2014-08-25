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
using System.Runtime.CompilerServices;
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
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b, System.Int32 c = (Literal: System.Int32 10) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void ImplicitIntToLong()
        {
            string source = "auto a = 5; long b = a;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int64 b = (LValueNode: Field a System.Int32) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TypeExceptionTest()
        {
            string source = "int a = 0.0;";
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Single 0))";
            try
            {
                TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Void$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void(System.Int32 arg)(CodeBlock: Symbols: () Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.Int32 4) Function: (LValueNode: Field Main $Functors.$System_Void$System_Int32))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void StringLiteralTest()
        {
            string source = @"auto a = ""word"";";
            string expected = "(ClassNode: Fields: System.String a = (Literal: System.String word) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void HelloWorld()
        {
            string source = @"
                auto Main = void()
                {
	                System.Console.WriteLine(""Hello, World!"");
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: () Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.String Hello, World!) Function: (MethodNode: Instance: null, Method: WriteLine))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void MoarArgs()
        {
            string source = @"
                auto Main = void()
                {
	                System.Console.WriteLine(""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"");
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: () Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!), (Literal: System.String Hello, World!) Function: (MethodNode: Instance: null, Method: WriteLine))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void NonStaticMethod()
        {
            string source = @"
                auto Main = void()
                {
	                auto a = 5;
                    auto str = a.ToString();
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: (System.Int32 a, System.String str) Nodes: ((Declaration: (LValueNode: LocalVariable a System.Int32) = (Literal: System.Int32 5)), (Declaration: (LValueNode: LocalVariable str System.String) = (MethodCall: Return: System.String Args:  Function: (MethodNode: Instance: (LValueNode: LocalVariable a System.Int32), Method: ToString)))))))";
            TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: (System.Boolean a, System.Int32 c) Nodes: ((Declaration: (LValueNode: LocalVariable a System.Boolean) = ), (Declaration: (LValueNode: LocalVariable c System.Int32) = (Literal: System.Int32 5)), (WhileBlock: Condition: (LValueNode: LocalVariable a System.Boolean), Block: (CodeBlock: Symbols: () Nodes: ((UnaryOp: VoidOperator (Assignment: (LValueNode: LocalVariable c System.Int32) = (BinaryOp: (LValueNode: LocalVariable c System.Int32) Addition (Literal: System.Int32 1))))))))))";
            TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: () Nodes: ((ConditionBlock: Condition: (Literal: System.Boolean True), True: (CodeBlock: Symbols: (System.Int32 a) Nodes: ((Declaration: (LValueNode: LocalVariable a System.Int32) = ))), False: (CodeBlock: Symbols: (System.Int32 b) Nodes: ((Declaration: (LValueNode: LocalVariable b System.Int32) = ))), (ConditionBlock: Condition: (Literal: System.Boolean False), True: (CodeBlock: Symbols: (System.Int32 c) Nodes: ((Declaration: (LValueNode: LocalVariable c System.Int32) = ))), False: ))))";
            TestParser(source, expected, lex);
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
            string expected = @"(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), System.Int32 b = (LValueNode: Field a System.Int32), $Functors.$System_Void$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void(System.Int32 arg)(CodeBlock: Symbols: (System.Int32 a, $Functors.$System_Int32$System_Int32$System_Single f) Nodes: ((Declaration: (LValueNode: LocalVariable a System.Int32) = (Literal: System.Int32 20)), (UnaryOp: VoidOperator (Assignment: (LValueNode: LocalVariable a System.Int32) = (Literal: System.Int32 10))), (Declaration: (LValueNode: LocalVariable f $Functors.$System_Int32$System_Int32$System_Single) = (MethodNode: Instance: null, Method: $Lambda_0)))))(Method: $Lambda_0 System.Int32(System.Int32 a, System.Single b)(CodeBlock: Symbols: (System.Single c) Nodes: ((Declaration: (LValueNode: LocalVariable c System.Single) = (BinaryOp: (LValueNode: FunctionArgument a System.Int32) Multiplication (LValueNode: FunctionArgument b System.Single))), (ReturnNode: (LValueNode: FunctionArgument a System.Int32))))))";
            TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: System.Int32 a = (Literal: System.Int32 5), $Functors.$System_Void$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void(System.Int32 b)(CodeBlock: Symbols: () Nodes: ((UnaryOp: VoidOperator (Assignment: (LValueNode: Field a System.Int32) = (BinaryOp: (LValueNode: Field a System.Int32) Addition (LValueNode: FunctionArgument b System.Int32)))), (UnaryOp: VoidOperator (Assignment: (LValueNode: FunctionArgument b System.Int32) = (BinaryOp: (LValueNode: FunctionArgument b System.Int32) Multiplication (LValueNode: Field a System.Int32)))), (UnaryOp: VoidOperator (Assignment: (LValueNode: Field a System.Int32) = (BinaryOp: (LValueNode: Field a System.Int32) Division (Literal: System.Int32 5)))), (UnaryOp: VoidOperator (Assignment: (LValueNode: Field a System.Int32) = (BinaryOp: (LValueNode: Field a System.Int32) Subtraction (Literal: System.Int32 8))))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestPrecedence()
        {
            string source = "auto a = 5 * 4 + 8 * (2 + 1);";
            string expected = "(ClassNode: Fields: System.Int32 a = (BinaryOp: (BinaryOp: (Literal: System.Int32 5) Multiplication (Literal: System.Int32 4)) Addition (BinaryOp: (Literal: System.Int32 8) Multiplication (BinaryOp: (Literal: System.Int32 2) Addition (Literal: System.Int32 1)))) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnValue()
        {
            string source = @"auto Main = int(int b)
                {
                    return b;
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Int32(System.Int32 b)(CodeBlock: Symbols: () Nodes: ((ReturnNode: (LValueNode: FunctionArgument b System.Int32))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUnaryOrder()
        {
            string source = @"
                auto i = 1;
                auto a = ++--i;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PreIncrement (UnaryOp: PreDecrement (LValueNode: Field i System.Int32))) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSuffixOrder()
        {
            string source = @"
                auto i = 1;
                auto a = i++--;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PostDecrement (UnaryOp: PostIncrement (LValueNode: Field i System.Int32))) Methods: )";
            TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PostIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (UnaryOp: PreIncrement (LValueNode: Field i System.Int32))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMixedSuffixPrefix()
        {
            string source = @"
                auto i = 1;
                auto a = -++i--;";
            string expected = "(ClassNode: Fields: System.Int32 i = (Literal: System.Int32 1), System.Int32 a = (UnaryOp: PostDecrement (UnaryOp: Negation (UnaryOp: PreIncrement (LValueNode: Field i System.Int32)))) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestStringPlusNumber()
        {
            string source = @"
                auto a = 5 + ""something"";";
            string expected = "(ClassNode: Fields: System.String a = (BinaryOp: (Literal: System.Int32 5) Addition (Literal: System.String something)) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestBinaryOps()
        {
            string source = @"
                auto i = 1 ^ 2;
                auto a = i & i;
                int c = i | a;";
            string expected = "(ClassNode: Fields: System.Int32 i = (BinaryOp: (Literal: System.Int32 1) BinaryXor (Literal: System.Int32 2)), System.Int32 a = (BinaryOp: (LValueNode: Field i System.Int32) BinaryAnd (LValueNode: Field i System.Int32)), System.Int32 c = (BinaryOp: (LValueNode: Field i System.Int32) BinaryOr (LValueNode: Field a System.Int32)) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestLogicalOps()
        {
            string source = @"
                auto i = true && false;
                auto a = i || true;";
            string expected = "(ClassNode: Fields: System.Boolean i = (BinaryOp: (Literal: System.Boolean True) LogicalAnd (Literal: System.Boolean False)), System.Boolean a = (BinaryOp: (LValueNode: Field i System.Boolean) LogicalOr (Literal: System.Boolean True)) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestComparison()
        {
            string source = @"
                auto i = true == false;
                auto a = i != true;
                auto c = 5 > 6;
                auto b = 8 <= 10;";
            string expected = "(ClassNode: Fields: System.Boolean i = (BinaryOp: (Literal: System.Boolean True) Equals (Literal: System.Boolean False)), System.Boolean a = (BinaryOp: (LValueNode: Field i System.Boolean) NotEquals (Literal: System.Boolean True)), System.Boolean c = (BinaryOp: (Literal: System.Int32 5) GreaterThan (Literal: System.Int32 6)), System.Boolean b = (BinaryOp: (Literal: System.Int32 8) LessEqualThan (Literal: System.Int32 10)) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnTypeFailure()
        {
            string source = @"
                auto Main = int(){return 4.0;};";
            string expected = "";
            try
            {
                TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Int32()(CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 4))))))";
            TestParser(source, expected, true);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Int32()(CodeBlock: Symbols: () Nodes: ((ConditionBlock: Condition: (Literal: System.Boolean True), True: (CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 1)))), False: (CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 0))))))))";
            TestParser(source, expected, lex);
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
                TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Int32()(CodeBlock: Symbols: () Nodes: ((ConditionBlock: Condition: (Literal: System.Boolean True), True: (CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 1)))), False: , (ReturnNode: (Literal: System.Int32 0))))))";
            TestParser(source, expected, lex);
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
            string expected = "(ClassNode: Fields: $Functors.$System_Int32 Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Int32()(CodeBlock: Symbols: () Nodes: ((CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 500))))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestScaryOperators()
        {
            string source = @"
                auto a = 1 * 2 / 3 * 4 % 5;";
            string expected = "(ClassNode: Fields: System.Int32 a = (BinaryOp: (BinaryOp: (BinaryOp: (BinaryOp: (Literal: System.Int32 1) Multiplication (Literal: System.Int32 2)) Division (Literal: System.Int32 3)) Multiplication (Literal: System.Int32 4)) Modulus (Literal: System.Int32 5)) Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctionType()
        {
            string source = @"
                void() a = void(){};";
            string expected = "(ClassNode: Fields: $Functors.$System_Void a = (MethodNode: Instance: null, Method: $a) Methods: (Method: $a System.Void()(CodeBlock: Symbols: () Nodes: ())))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctionType2()
        {
            string source = @"
                int(float) a = int(float x){return 4;};";
            string expected = "(ClassNode: Fields: $Functors.$System_Int32$System_Single a = (MethodNode: Instance: null, Method: $a) Methods: (Method: $a System.Int32(System.Single x)(CodeBlock: Symbols: () Nodes: ((ReturnNode: (Literal: System.Int32 4))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnVoid()
        {
            string source = @"
                auto Main = void()
                {  
                    return 5;
                };";
            string expected = "";
            try
            {
                TestParser(source, expected, lex);
            }
            catch(TypeException)
            {
                return;
            }
            Assert.Fail("Should've failed, returning 5 in void()");
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImport()
        {
            string source = @"
                use System;";
            string expected = "(ClassNode: Fields:  Methods: )";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUseImport()
        {
            string source = @"
                use System;
                auto Main = void()
                {
	                Console.WriteLine(""Hello, World!"");
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: () Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.String Hello, World!) Function: (MethodNode: Instance: null, Method: WriteLine))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMultipleImport()
        {
            string source = @"
                use System;
                use System.IO;
                auto Main = void()
                {
	                Console.WriteLine(""Hello, World!"");
                    File.Exists("""");
                };";
            string expected = "(ClassNode: Fields: $Functors.$System_Void Main = (MethodNode: Instance: null, Method: $Main) Methods: (Method: $Main System.Void()(CodeBlock: Symbols: () Nodes: ((MethodCall: Return: System.Void Args: (Literal: System.String Hello, World!) Function: (MethodNode: Instance: null, Method: WriteLine)), (UnaryOp: VoidOperator (MethodCall: Return: System.Boolean Args: (Literal: System.String ) Function: (MethodNode: Instance: null, Method: Exists)))))))";
            TestParser(source, expected, lex);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMethodInferrence()
        {
            string source = @"
                auto a = 5;
                auto Main = void()
                {
                    string() func = a.ToString;
	                System.Console.WriteLine(func());
                };";
            string expected = "";
            TestParser(source, expected, lex);
        }
        private void TestParser(string source, string expected, bool lex, [CallerMemberName]string name = "")
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
