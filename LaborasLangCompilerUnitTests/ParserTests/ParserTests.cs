﻿//#define REWRITE
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompilerUnitTests.ILTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private const string path = @"..\..\ParserTests\Trees\";

        [TestMethod, TestCategory("Parser")]
        public void FieldDeclarationTest()
        {
            string source = @"
                auto a = 5; 
                int b; 
                int c = 10;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void FieldModifierTest()
        {
            string source = @"
                private auto a = 5; 
                public int b; 
                public noinstance mutable int c = 10;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void ImplicitIntToLong()
        {
            string source = "auto a = 5; long b = a;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void FunctorTypeTest()
        {
            string source = @"
                int() a;
                void(float(double), int) b;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser"), ExpectedException(typeof(TypeException), "Assigned double to int")]
        public void TypeExceptionTest()
        {
            string source = "int a = 0.0;";
            CanParse(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void MethodCallTest()
        {
            string source = @"
                auto Main = void(int arg)
                {
	                Main(4);
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void StringLiteralTest()
        {
            string source = @"auto a = ""word"";";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void MethodDeclaration()
        {
            string source = @"
                auto Main = void()
                {
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void DeclarationInsideMethod()
        {
            string source = @"
                auto Main = void()
                {
                    auto c = 20;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void HelloWorld()
        {
            string source = @"
                auto Main = void()
                {
	                System.Console.WriteLine(""Hello, World!"");
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void MoarArgs()
        {
            string source = @"
                auto Main = void()
                {
	                System.Console.WriteLine(""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"", ""Hello, World!"");
                };";
            CompareTrees(source);
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
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void WhileLiteral()
        {
            string source = @"
                auto Main = void()
                {
	                while(true)
                    {
                    }
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void WhileSymbol()
        {
            string source = @"
                auto Main = void()
                {
                    bool a = true;
	                while(a)
                    {
                    }
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void WhileExpression()
        {
            string source = @"
                auto Main = void()
                {
                    int a = 8;
	                while(a < 10)
                    {
                    }
                };";
            CompareTrees(source);
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
            CompareTrees(source);
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
            CompareTrees(source);
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
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestPrecedence()
        {
            string source = "auto a = 5 * 4 + 8 * (2 + 1);";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnValue()
        {
            string source = @"auto Main = int(int b)
                {
                    return b;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUnaryOrder()
        {
            string source = @"
                auto i = 1;
                auto a = ++--i;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSuffixOrder()
        {
            string source = @"
                auto i = 1;
                auto a = i++--;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMixedSuffixPrefix()
        {
            string source = @"
                auto i = 1;
                auto a = -++i--;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestStringPlusNumber()
        {
            string source = @"
                auto a = 5 + ""something"";";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestBinaryOps()
        {
            string source = @"
                auto i = 1 ^ 2;
                auto a = i & i;
                int c = i | a;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestLogicalOps()
        {
            string source = @"
                auto i = true && false;
                auto a = i || true;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestComparison()
        {
            string source = @"
                auto i = true == false;
                auto a = i != true;
                auto c = 5 > 6;
                auto b = 8 <= 10;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser"), ExpectedException(typeof(TypeException), "Returned double instead of int")]
        public void TestReturnTypeFailure()
        {
            string source = @"
                auto Main = int(){return 4.0;};";
            CanParse(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnTypeSuccess()
        {
            string source = @"
                auto Main = int(){return 4;};";
            CompareTrees(source);
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
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser"), ExpectedException(typeof(ParseException), "Not all method paths return")]
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
            CanParse(source);
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
            CompareTrees(source);
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
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestScaryOperators()
        {
            string source = @"
                auto a = 1 * 2 / 3 * 4 % 5;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctionType()
        {
            string source = @"
                void() a = void(){};";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctionType2()
        {
            string source = @"
                int(float) a = int(float x){return 4;};";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser"), ExpectedException(typeof(TypeException), "Returned value in a void method")]
        public void TestReturnVoid()
        {
            string source = @"
                auto Main = void()
                {  
                    return 5;
                };";
            CanParse(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImport()
        {
            string source = @"
                use System;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUseImport()
        {
            string source = @"
                use System.Collections;
                auto Main = void()
                {
	                ArrayList foo;
                };";
            CompareTrees(source);
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
            CompareTrees(source);
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
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestAccessModifiers()
        {
            string source = @"
                private auto bar = void()
                {
                };
                public auto foo = void()
                {
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestEntryModifiers()
        {
            string source = @"
                entry auto foo = void()
                {
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestConstructorCall()
        {
            string source = @"
                auto foo = System.Collections.ArrayList();";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestConstructorCallArgs()
        {
            string source = @"
                auto foo = System.Collections.ArrayList(5);";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestHigherOrderFunctor()
        {
            string source = @"
                auto foo = void()()
                {
                    return void(){};
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestCallHigherOrderFunctor()
        {
            string source = @"
                auto foo = void()()
                {
                    return void(){};
                };
                auto bar = void()
                {
                    foo()();
                }";
            CompareTrees(source);
        }
        private static void CompareTrees(string source, [CallerMemberName] string name = "")
        {
            var compilerArgs = CompilerArguments.Parse(new[] { name + ".ll" });
            var assembly = new AssemblyEmitter(compilerArgs);
            var file = path + name;
            using(var tree = Lexer.Lexer.Lex(source))
            {
                Parser parser = new Parser(assembly, tree, name, false);
                string result = parser.Root.ToString(0);
#if REWRITE
                System.IO.File.WriteAllText(file, result);
#else
                string expected = "";
                try
                {
                    expected = System.IO.File.ReadAllText(file);
                }
                catch { }
                Assert.AreEqual(expected, result);
#endif
            }
        }
        public static void CanParse(string source, [CallerMemberName] string name = "")
        {
            var compilerArgs = CompilerArguments.Parse(new[] { name + ".ll" });
            var assembly = new AssemblyEmitter(compilerArgs);
            using(var tree = Lexer.Lexer.Lex(source))
            {
                Parser parser = new Parser(assembly, tree, name, false);
            }
        }
    }
}
