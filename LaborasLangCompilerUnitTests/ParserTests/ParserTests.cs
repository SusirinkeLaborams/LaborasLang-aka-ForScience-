using LaborasLangCompiler.Common;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser.Impl;
using LaborasLangCompilerUnitTests.CodegenTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace LaborasLangCompilerUnitTests.ParserTests
{
    [TestClass]
    public class ParserTests : ParserTestBase
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext ctx)
        {
            // avoid contract violation kill the process  
            Contract.ContractFailed += new EventHandler<ContractFailedEventArgs>(Contract_ContractFailed);
        }

        static void Contract_ContractFailed(object sender, System.Diagnostics.Contracts.ContractFailedEventArgs e)
        {
            e.SetHandled();
            Assert.Fail("{0}: {1} {2}", e.FailureKind, e.Message, e.Condition);
        } 

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
        [TestMethod, TestCategory("Parser")]
        public void TypeExceptionTest()
        {
            string source = "int a = 0.0;";
            CompareTrees(source, ErrorCode.TypeMissmatch.Enumerate());
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
        public void CharLiteralTest()
        {
            string source = @"auto a = 'ė';";
            CompareTrees(source);
        }

        [TestMethod, TestCategory("Parser")]
        public void MultipleCharLiteralTest()
        {
            string source = @"auto a = 'ėėė';";
            CompareTrees(source, new[] { ErrorCode.MultipleCharacterLiteral });
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
                int a = 5;
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
        public void TestEmptyReturn()
        {
            string source = @"auto Main = void()
                {
                    return;
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
        public void TestMixedPrefix()
        {
            string source = @"
                auto i = 1;
                auto a = -++i;";
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
        [TestMethod, TestCategory("Parser")]
        public void TestReturnTypeFailure()
        {
            string source = @"
                auto Main = int(){return 4.0;};";
            CompareTrees(source, Utils.Enumerate(ErrorCode.TypeMissmatch, ErrorCode.MissingReturn));
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
            CompareTrees(source, ErrorCode.MissingReturn.Enumerate());
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
        [TestMethod, TestCategory("Parser")]
        public void TestReturnVoid()
        {
            string source = @"
                auto Main = void()
                {  
                    return 5;
                };";
            CompareTrees(source, ErrorCode.TypeMissmatch.Enumerate());
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
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestLocalVariableVoid()
        {
            string source = @"
                auto foo = void()
                {
                    void a;
                };";
            CompareTrees(source, ErrorCode.VoidDeclaration.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFieldVoid()
        {
            string source = @"
                void a;";
            CompareTrees(source, ErrorCode.VoidDeclaration.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestVoidParamFunctorType()
        {
            string source = @"
                mutable void(void) foo;";
            CompareTrees(source, ErrorCode.IllegalMethodParam.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestVoidParamMethod()
        {
            string source = @"
                auto foo = void(void a)
                {
                };";
            CompareTrees(source, ErrorCode.IllegalMethodParam.Enumerate());
        }
        [TestMethod, TestCategory("Parser"), TestCategory("Lexer: SyntaxMatcher")]
        public void TestUnaryOnCall()
        {
            string source = @"
                mutable int() foo;
                auto a = -foo();";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestParenthesisProduceExplosions()
        {
            string source = @"
                mutable int() foo;
                auto a = -(foo());";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUnaryInvalid1()
        {
            string source = @"
                mutable int() foo;
                auto a = foo()++;";
            CompareTrees(source, ErrorCode.NotAnLValue.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUnaryInvalid2()
        {
            string source = @"
                mutable int() foo;
                auto a = ++foo();";
            CompareTrees(source, ErrorCode.NotAnLValue.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestAssignToUnary()
        {
            string source = @"
                auto foo = void()
                {
                    int a = 5;
                    ++a = 8;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestLiteralIntToLong()
        {
            string source = @"
                long a = 5;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReturnMemberMethod()
        {
            //return foo would result in null ptr before
            string source = @"
                auto foo = void()
                {
                };

                auto getFoo = void()()
                {
	                return foo;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestConstLocal()
        {
            string source = @"
                auto foo = void()
                {
                    const int bar = 5;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestConstAutoLocal()
        {
            string source = @"
                auto foo = void()
                {
                    const auto bar = ""bar"";
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUninitializedLocal()
        {
            string source = @"
                auto foo = void()
                {
                    const int bar;
                };";
            CompareTrees(source, ErrorCode.MissingInit.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMutableLocal()
        {
            string source = @"
                auto foo = void()
                {
                    mutable int bar;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestAsignToConstLocal()
        {
            string source = @"
                auto foo = void()
                {
                    const int bar = 5;
                    bar = 8;
                };";
            CompareTrees(source, ErrorCode.NotAnLValue.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestPrivateLocal()
        {
            string source = @"
                auto foo = void()
                {
                    private const int bar = 5;
                };";
            CompareTrees(source, ErrorCode.InvalidVariableMods.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestVoidEntry()
        {
            string source = @"
                entry auto foo = void()
                {
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestIntEntry()
        {
            string source = @"
                entry auto foo = int()
                {
                    return 0;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUIntEntry()
        {
            string source = @"
                entry auto foo = uint()
                {
                    uint bar;
                    return bar;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFloatEntry()
        {
            string source = @"
                entry auto foo = float()
                {
                };";
            CompareTrees(source, ErrorCode.InvalidEntryReturn.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestReadProperty()
        {
            string source = @"
                auto lst = System.Collections.ArrayList();
                auto count = lst.Count;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestWriteNoSetterProperty()
        {
            string source = @"
                auto lst = System.Collections.ArrayList();
                auto foo = void()
                {
                    lst.Count = 5;
                };";
            CompareTrees(source, ErrorCode.NotAnLValue.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestWriteProperty()
        {
            string source = @"
                auto lst = System.Collections.ArrayList();
                auto foo = void()
                {
                    lst.Capacity = 5;
                };";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestTwoFiles()
        {
            string file1 = @"auto foo = 5;";
            string file2 = @"auto foo = ""asfasfa"";";
            CompareTrees(Utils.Enumerate(file1, file2), Utils.Enumerate("file1", "file2"));
        }
        [TestMethod, TestCategory("Parser")]
        public void TestTwoFilesFieldVisibility()
        {
            string file1 = @"public auto foo = 5;";
            string file2 = @"auto foo = file1.foo;";
            CompareTrees(Utils.Enumerate(file1, file2), Utils.Enumerate("file1", "file2"));
        }
        [TestMethod, TestCategory("Parser")]
        public void TestTwoFilesFieldCircularVisibility()
        {
            //one of the foos is not found because type inferrence delays field declaration
            string file1 = @"public auto foo = file2.foo;";
            string file2 = @"public auto foo = file1.foo;";
            CompareTrees(Utils.Enumerate(file1, file2), Utils.Enumerate("file1", "file2"), ErrorCode.SymbolNotFound.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestTwoFilesMethodVisibility()
        {
            string file1 = @"public auto foo = file2.foo();";
            string file2 = @"public auto foo = int(){return 4;};";
            CompareTrees(Utils.Enumerate(file1, file2), Utils.Enumerate("file1", "file2"));
        }
        [TestMethod, TestCategory("Parser")]
        public void TestTwoFilesVisibilityMoar()
        {
            string file1 = @"public auto foo = void(){file2.foo();};";
            string file2 = @"public auto foo = void(){file1.foo();};";
            CompareTrees(Utils.Enumerate(file1, file2), Utils.Enumerate("file1", "file2"));
        }
        [TestMethod, TestCategory("Parser")]
        public void TestIntLiteralToULong()
        {
            string source = @"ulong foo = 5;";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImplicitCallResultCast()
        {
            string source = @"
                auto foo = int(){return 4;};
                long bar = foo();";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestValueCreation()
        {
            string source = @"
                auto foo = int();";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestBinaryOpOverload()
        {
            string source = @"
                auto foo = ""foo"" == ""bar"";";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayType()
        {
            string source = @"
                int[] foo;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMultidimArrayType()
        {
            string source = @"
                int[,,] foo;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctorArrayType()
        {
            string source = @"
                int()[] foo;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctorReturningArrayType()
        {
            string source = @"
                mutable int[]() foo;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestHorribleMixedType()
        {
            string source = @"
                int[]()[,](int, float[])[] foo;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestIncrement()
        {
            string source = @"
                entry auto main = void()
                {
                    auto a = 5;
                    a++;
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSingleElementArrayCreationInitialized()
        {
            string source = @"
                auto foo = int[1]{5};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestMultiElementArrayCreationInitialized()
        {
            string source = @"
                auto foo = int[3]{5, 6, 7};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayCreationInitializedImplicitSize()
        {
            string source = @"
                auto foo = int[]{5, 6};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayCreationMatrixInitializedImplicitSize()
        {
            string source = @"
                auto foo = int[,]{{1}, {2}};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayCreationMatrixInitialized()
        {
            string source = @"
                auto foo = int[1, 1]{{1}, {2}};
            ";
            CompareTrees(source, ErrorCode.ArrayDimMissmatch.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestEmptyInitializedArray()
        {
            string source = @"
                auto foo = int[]{};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestJaggedArray()
        {
            string source = @"
                auto foo = int[][]{int[]{}, int[]{}};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayCreationLiteralSize()
        {
            string source = @"
                auto foo = int[10];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayCreationMethodSize()
        {
            string source = @"
                auto size = int(){return 4;};
                auto foo = int[size()];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayCreationMethodSizeInitialized()
        {
            string source = @"
                auto size = int(){return 4;};
                auto foo = int[size()]{1, 2, 3, 4};
            ";
            CompareTrees(source, ErrorCode.NotLiteralArrayDims.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImplicitArraySingleDimIntegers()
        {
            string source = @"
                auto foo = {5, 8, 695, 0};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImplicitArraySingleDimDoubles()
        {
            string source = @"
                auto foo = {5.0, 8.0, 695.0, 0.0};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImplicitArraySingleDimStrings()
        {
            string source = @"
                auto foo = {""foo"", ""bar""};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImplicitArraySingleDimObjects()
        {
            string source = @"
                use System.Collections;
                auto foo = {ArrayList(), ArrayList(), ArrayList()};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestImplicitArraySingleDimObjectsDifferentTypes()
        {
            string source = @"
                use System.Collections;
                auto foo = {ArrayList(), ArrayList(), Hashtable()};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccess()
        {
            string source = @"
                int[] foo;
                auto bar = foo[5];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessCallArg()
        {
            string source = @"
                int[] foo;
                mutable int() ind;
                auto bar = foo[ind()];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessOnCall()
        {
            string source = @"
                mutable int[]() foo;
                auto bar = foo()[5];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestIndexOpGet()
        {
            string source = @"
                use System.Collections;
                auto list = ArrayList();
                auto bar = list[5];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayOfLists()
        {
            string source = @"
                use System.Collections;
                auto bar = ArrayList[5];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestIndexOpSet()
        {
            string source = @"
                use System.Collections;
                auto list = ArrayList();
                auto func = void()
                {
                    list[5] = ""explosions"";
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessMatrix()
        {
            string source = @"
                int[,] foo;
                auto bar = foo[5, 6];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessMatrixCallArgs()
        {
            string source = @"
                int[,] foo;
                mutable int() ind;
                auto bar = foo[ind(), ind()];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessJagged1()
        {
            string source = @"
                int[][] foo;
                auto bar = foo[5];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessJagged2()
        {
            string source = @"
                int[][] foo;
                auto bar = foo[5][6];
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayAccessJaggedInvalid()
        {
            string source = @"
                int[][] foo;
                auto bar = foo[5][6][7];
            ";
            CompareTrees(source, ErrorCode.CannotIndex.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestArrayInPlaceReassign()
        {
            string source = @"
                int[] foo;
                auto func = void()
                {
                    foo[0] = foo[1];
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSimpleForLoop()
        {
            string source = @"
                auto func = void()
                {
                    int num = 0;
                    for(int i = 0; i < 5; i++)
                    {
                        num += i;
                    }
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSimpleForLoopOutsideIndex()
        {
            string source = @"
                auto func = void()
                {
                    int num = 0;
                    int i;
                    for(i = 0; i < 5; i++)
                    {
                        num += i;
                    }
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestSimpleForLoopEmpty()
        {
            string source = @"
                auto func = void()
                {
                    for(int i = 0; i < 5; i++);
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestInfiniteFor()
        {
            string source = @"
                auto func = void()
                {
                    for(;;){}
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestInfiniteForNoBlock()
        {
            string source = @"
                auto func = void()
                {
                    for(;;);
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForNoInit()
        {
            string source = @"
                auto func = void()
                {
                    int i = 0;
                    for(; i < 5; i++){}
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForNoCondition()
        {
            string source = @"
                auto func = void()
                {
                    for(int i = 0; ; i++){}
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForNoIncrement()
        {
            string source = @"
                auto func = void()
                {
                    for(int i = 0; i < 5;){}
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForOnlyCondition()
        {
            string source = @"
                auto func = void()
                {
                    bool loop = true;
                    for(;loop;){}
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullField()
        {
            string source = @"
                string str = null;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullAutoField()
        {
            string source = @"
                auto str = null;
            ";
            CompareTrees(source, ErrorCode.InferrenceFromTypeless.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullAutoVar()
        {
            string source = @"
                auto main = void()
                {
                    auto str = null;
                };
            ";
            CompareTrees(source, ErrorCode.InferrenceFromTypeless.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullVar()
        {
            string source = @"
                auto main = void()
                {
                    string str = null;
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullParameter()
        {
            string source = @"
                mutable void(string) foo;
                auto main = void()
                {
                    foo(null);
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullInImplicitArray()
        {
            string source = @"
                auto arr = {""foo"", ""bar"", null};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestOnlyNullInImplicitArray()
        {
            string source = @"
                auto arr = {null, null, null};
            ";
            CompareTrees(source, ErrorCode.InferrenceFromTypeless.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullInExplicitArray()
        {
            string source = @"
                auto arr = string[]{""foo"", ""bar"", null};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestOnlyNullInExplicitArray()
        {
            string source = @"
                auto arr = string[]{null, null, null};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestDotOnNull()
        {
            string source = @"
                auto foo = null.Bar();
            ";
            CompareTrees(source, ErrorCode.NullInsideDot.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullMatrixElement()
        {
            string source = @"
                auto foo = {{null, ""foo""}, {""bar"", null}};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestOnlyNullMatrixElement()
        {
            string source = @"
                auto foo = {{null, null}, {null, null}};
            ";
            CompareTrees(source, ErrorCode.InferrenceFromTypeless.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullMatrixRow()
        {
            string source = @"
                auto foo = {{null, null}, {""bar"", ""foo""}};
            ";
            CompareTrees(source, ErrorCode.InferrenceFromTypeless.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestCallOnNull()
        {
            string source = @"
                auto foo = null();
            ";
            CompareTrees(source, ErrorCode.NotCallable.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachArrayList()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    for(auto foo in ArrayList());
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachIList()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    IList lst = ArrayList();
                    for(auto foo in lst);
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachIListInvalidVarialbeType()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    IList lst = ArrayList();
                    for(int foo in lst);
                };
            ";
            CompareTrees(source, ErrorCode.TypeMissmatch.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachOverInteger()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    int lst = 5;
                    for(int foo in lst);
                };
            ";
            CompareTrees(source, ErrorCode.InvalidForEachCollection.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachVarialbeScope()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    auto lst = int[]{};
                    for(int foo in lst);
                    foo++;
                };
            ";
            CompareTrees(source, ErrorCode.SymbolNotFound.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachArray()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    auto lst = int[]{};
                    for(int foo in lst);
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachVarialbeRead()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    auto lst = int[]{};
                    for(int foo in lst)
                    {
                        auto tmp = foo;
                    }
                };
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachVarialbeWrite()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    auto lst = int[]{};
                    for(int foo in lst)
                    {
                        foo = 5;
                    }
                };
            ";
            CompareTrees(source, ErrorCode.NotAnLValue.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestForEachOverNull()
        {
            string source = @"
                use System.Collections;
                auto main = void()
                {
                    for(int foo in null)
                    {
                        foo = 5;
                    }
                };
            ";
            CompareTrees(source, ErrorCode.InvalidForEachCollection.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestUpCast()
        {
            string source = @"
                object foo = (object)""str"";
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestDownCast()
        {
            string source = @"
                string bar = (string)object();
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNestedCast()
        {
            string source = @"
                string bar = (string)(object)""foo"";
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestIllegalCast()
        {
            string source = @"
                string bar = (string)5;
            ";
            CompareTrees(source, ErrorCode.IllegalCast.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctorUpCast()
        {
            string source = @"
                object foo = (object)void(){};
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestFunctorDownCast()
        {
            string source = @"
                object foo = (object)void(){};
                auto bar = (void())foo;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestCallOnCast()
        {
            string source = @"
                object foo = (object)void(){};
                auto bar = ((int())foo)();
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullCast()
        {
            string source = @"
                auto str = (string)null;
            ";
            CompareTrees(source);
        }
        [TestMethod, TestCategory("Parser")]
        public void TestNullToValueCast()
        {
            string source = @"
                auto str = (int)null;
            ";
            CompareTrees(source, ErrorCode.IllegalCast.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestVoidFieldByInferrence()
        {
            string source = @"
                mutable void() func;
                auto foo = func();
            ";
            CompareTrees(source, ErrorCode.NotAnRValue.Enumerate());
        }
        [TestMethod, TestCategory("Parser")]
        public void TestVoidLocalByInferrence()
        {
            string source = @"
                mutable void() func;
                auto main = void()
                {
                    auto local = func();
                };
            ";
            CompareTrees(source, ErrorCode.NotAnRValue.Enumerate());
        }
    }
}