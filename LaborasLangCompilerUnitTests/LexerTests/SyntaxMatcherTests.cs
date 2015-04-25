using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexer;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Text;
using System.IO;
using Lexer.Containers;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class SyntaxMatcherTests : SyntaxMatcherTestBase
    {
        private const int timeout = 0;

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testCreateArray()
        {
            var source = @"int[] a = bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testMultidimensionalIndexAccess()
        {
            var source = @"a = foo[1, bar()];";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testValidButStupidFunctionReturnsArray()
        {
            var source = @"a = foo[,](){return bar;};";
            ExecuteTest(source);
        }
        
        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testIndexAccess()
        {
            var source = @"auto a = foo[1];";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testCreateEmptyArray()
        {
            var source = @"int[] a = int[5];";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testCreateEmptyMultidimensionalArray()
        {
            var source = @"int[,] a = int[5,5];";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testDeclareEmptyArray()
        {
            var source = @"int[] a;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testFunctionArrayType()
        {
            var source = @"int()[] a;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testLambdaArrayInitialization()
        {
            var source = @"auto a = void()[]{void(){return; }, void(){return;}};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testFunctionArrayInitialization()
        {
            var source = @"auto a = void()[]{1, 2};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayInitialization()
        {
            var source = @"auto a = int[]{1, 2, 3};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayCreation()
        {
            var source = @"auto a = int[];";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testEmptyCodeBlock()
        {
            var source = @"
            {}
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayLiteral()
        {
            var source = @"
            a = { 1 };
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testSingleElementArrayCreation()
        {
            var source = @"
            a = int[1]{ 1 };
            ";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayWithoutInitializerList()
        {
            var source = @"
            a = int[];
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayElementAccess()
        {
            var source = @"
            a = value[5];
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testJaggedArrayAccess()
        {
            var source = @"
            a = value[5][6];
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testJaggedArrayInitialization()
        {
            var source = @"
            a = int[][]{int[0], int[0]};
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testCallOnArrayAccess()
        {
            var source = @"
            a = foo[0]();
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayAccessOnCall()
        {
            var source = @"
            a = foo()[0];
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testMixedArrayAccessAndCalls()
        {
            var source = @"
            a = foo[0]()[1, 5]()()[2][3]();
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayLiteralWithTwoValues()
        {
            var source = @"
            a = { 1, 1 };
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testEmptyArray()
        {
            var source = @"
            a = { };
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testImplicitMatrixInitialization()
        {
            var source = @"
            a = { {1, 2}, {3, 4} };
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testExplicitMatrixInitialization()
        {
            var source = @"
            a = int[,]{ {1, 2}, {3, 4} };
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testExplicitSizeMatrixInitialization()
        {
            var source = @"
            a = int[2, 2]{ {1, 2}, {3, 4} };
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testImplicitArrayLiteralAsArg()
        {
            var source = @"
            foo({5}, {6, 7});
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testArrayLiteralAsArg()
        {
            var source = @"
            foo(int[1], float[2]);
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testExplicitArrayLiteralAsArg()
        {
            var source = @"
            foo(int[]{5}, int[]{8, 9});
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestMethod1()
        {

            var source = @"
int a;
int b;
a = b;
a = 5;
";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testFunctorReturningArrayOfFunctorsType()
        {
            var source = @"
            void()[]() foo;
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testDeclaredFunctorReturningArrayOfFunctorsType()
        {
            var source = @"
            void()[](int) foo = void()[](int a){};
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestFunctionDeclarationAndCall()
        {

            var source = @"void(int a){a = 5;}
(5)
()
()
();
";
            ExecuteTest(source);
        }




        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void DeclareFunctionTest()
        {
            var source = @"int(int, bool) foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void DeclareEntryFunctionTest()
        {
            var source = @"entry int() foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void FunctionCall()
        {
            var source =
@"
foo();
foo(bar);
foo(foo(), bar);
foo()()();";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void FullSymbolFunctionCall()
        {
            var source = @"Console.WriteLine(5);";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void DeclareFunctionTestNoParameters()
        {
            var source = @"int() foo;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void DeclareAndAssignFunction()
        {
            var source = @"auto foo = int() {foo();};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void DeclareAndAssignFunctionWithParameter()
        {
            var source = @"auto foo = int(param p) {foo();};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void DeclareVariable()
        {
            var source = @"int foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanAnd()
        {
            var source = @"i = i && false;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanOr()
        {
            var source = @"a = i || true;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_NoArguments()
        {
            var source = @"foo = int() { bar(); };";
            ExecuteTest(source);
        }
        
        [TestMethod, TestCategory("Lexer: SyntaxMatcher"), TestCategory("Lexer")]
        public void TestSymbolIsNotACast()
        {
            string source = @"auto a = -(foo());";

            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void BasicForeach()
        {
            var source = @"for(auto i in foo);";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void BasicFor()
        {
            var source = @"for(auto i = 0; i < 5; i++);";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void ForWithoutDeclaration()
        {
            var source = @"for(; i < 5; i++);";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void EmptyFor()
        {
            var source = @"for(;;);";
            ExecuteTest(source);
        }



        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void BasicForWithAssignment()
        {
            var source = @"for(i = 0; i < 5; i++);";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_OneArgument()
        {
            var source = @"foo = int(int a) { bar(); };";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_TwoArguments()
        {
            var source = @"foo = int(int a, float c) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_MultipleArguments()
        {
            var source = @"foo = int(int a, float c, float d,  float d2) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void MultipleFunctionCallTest()
        {
            var source = @"
foo().bar();
foo().bar = 5;
(foo + 5).bar();
";
            ExecuteTest(source);
        }
       
        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void IfTest()
        {
            var source = @"
            if(somehting) 
            {
                stuff();
            }
            else 
            {
            stuff2();
            }

            if(something)
            {
                stuff();
            }
            ";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestFunctionDeclaration()
        {
            var source = @"
            auto Main = void()
            {
                
            };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestIntegerLiteralToken()
        {
            var source = "foo = 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestNegativeIntegerToken()
        {
            var source = "foo = -1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestNestedNegativeOps()
        {
            var source = "foo = - - - 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestMixedBinaryAndUnaryMinus()
        {
            var source = "foo = 5 - - 1 - 5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestCharLiteralToken()
        {
            var source = @"foo = 'bar';";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanLiteralTrue()
        {
            var source = @"foo = true;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanLiteralFalse()
        {
            var source = @"foo = false;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestStringLiteralToken()
        {
            var source = @"foo = ""bar"";";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestFloatLiteralToken()
        {
            var source = "foo = 1.1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestWhileLoop()
        {
            string source =
          @"while(condition) function();
            while(condition()) {function();}";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestUnaryNegation()
        {
            var source = "foo = -bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestAddition()
        {
            var source = @"foo = 1 + bar;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestValue()
        {
            var source = @"1";

            AssertCanBeLexedAs(source, TokenType.Operand);
            AssertCanBeLexedAs(source, TokenType.ParenthesesNode);
            AssertCanBeLexedAs(source, TokenType.PrefixNode);
            AssertCanBeLexedAs(source, TokenType.PostfixNode);
            AssertCanBeLexedAs(source, TokenType.InfixNode);
            AssertCanBeLexedAs(source, TokenType.Value);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestBinaryAnd()
        {
            var source = @"foo = 1 & bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestBinaryOr()
        {
            var source = @"foo = 1 | bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestSubtraction()
        {
            var source = @"foo = bar - 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestMultiplication()
        {
            var source = @"foo = bar * 1.5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestDivision()
        {
            var source = @"foo = 1.5 / bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestAdditionAndMultiplicationOrder()
        {
            var source = @"f().oo = 5 + 1.2 * bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestSubtractionAndDivisionOrder()
        {
            var source = @"foo = 1.5  - bar / 15;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestSinglePrefix()
        {
            var source = "foo = ++i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestDoublePrefix()
        {
            var source = "foo = ++ ++i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestMultiplePrefixes()
        {
            var source = "foo = --++!++--i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestSingleSuffix()
        {
            var source = "foo = i++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestDoubleSuffix()
        {
            var source = "foo = i++ ++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestMultipleSuffixes()
        {
            var source = "foo = i++--++--++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestParentheses()
        {
            var source = "foo = ((1 + 2) + 3 + bar(2 + 3));";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestInfixOperatorPrecedence()
        {
            var source = "foo = 1 * 2 / 3 % 4;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestPeriodPrecedence()
        {
            var source = "foo = a.b.c.d.e * 2 / 3 % 4;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestRightAssociativeOperators()
        {
            var source = "foo = bar = foobar = foobar2k;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestPrefixSuffix()
        {
            var source = "foo = ++i++;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestCast()
        {
            var source = "foo = (bar) foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestSumCast()
        {
            var source = "foo = (foo) (foo + 6);";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestMultipleCasts()
        {
            var source = "foo = (foo)(bar) foo;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testFunctionCast()
        {
            var source = "foo = (bar(int)) foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void testCallOnCast()
        {
            var source = "foo = ((bar(int)) foo)(5);";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestNestedAddition()
        {
            var source = "foo = a + b + a;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestUseNode()
        {
            var source =
@"use System;
use System.Diagnostics;
use Windows.Security.Cryptography.Core; use Windows;";

            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestLogicalOperators()
        {
            var source =
@"a = true && false;
b = false || true;
a ||= true;
b &&= a;";

            ExecuteTest(source);
        }
        
        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestFunctionCallOnOperatorResult()
        {
            var source = @"(5 + 6).ToString(aaa);";

            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestFunctionCallInsideCondition()
        {
            var source = "if (IsEven(i)) {}";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestReturn()
        {
            var source = @"
        return;
        return 5;";
            ExecuteTest(source);
        }

        
        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestReturnFunction()
        {
            var source = @"
auto foo = void()()
{
    return void(){};
};
";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestAssignToIncrement()
        {
            var source = @" ++foo = 5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestAssignToFunctionResult()
        {
            var source = @"foo().bar = 5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestAssignToSum()
        {
            var source = @"(foo + bar) = 5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestErrorousStatement()
        {
            var source = @"sudo bring me beer;";
            ExecuteFailingTest(source);
        }
        
        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestErrorousBlock()
        {
            var source = @"
            auto foo = + {
                should = work;
            };
            int a = 0;
            ";
            ExecuteFailingTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestErrorousStatements()
        {
            var source = @"int i = 4;
sudo bring me beer;";
            ExecuteFailingTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: SyntaxMatcher"), Timeout(timeout)]
        public void TestNull()
        {
            var source = @"
                int i = null;
                null = null + null;
                foo(null);
                null(null);
                ";
            ExecuteTest(source);
        }    
    }
}
