// #define REMATCH

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
    public class SyntaxMatcherTests
    {
        private const int timeout = 0;
        private const string Path = @"..\..\LexerTests\Tokens\";

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
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


        #region Tests

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareFunctionTest()
        {
            var source = @"int(int, bool) foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void FunctionCall()
        {
            var source =
            @"
            foo();
            foo(bar);
            foo(foo(), bar);";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareFunctionTestNoParameters()
        {
            var source = @"int() foo;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareAndAssignFunction()
        {
            var source = @"auto foo = int() {foo();};";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareAndAssignFunctionWithParameter()
        {
            var source = @"auto foo = int(param p) {foo();};";
            ExecuteTest(source);
        }
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void DeclareVariable()
        {
            var source = @"int foo;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanAnd()
        {
            var source = @"i = i && false;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanOr()
        {
            var source = @"a = i || true;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_NoArguments()
        {
            var source = @"foo = int() { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_OneArgument()
        {
            var source = @"foo = int(int a) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_TwoArguments()
        {
            var source = @"foo = int(int a, float c) { bar(); };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void AssignFunctionTest_MultipleArguments()
        {
            var source = @"foo = int(int a, float c, float d,  float d2) { bar(); };";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void HelloWorldTest()
        {
            var source = @"
            auto Main = void()
            {
                System.Console.WriteLine('Hello, world');
                System.Console.ReadKey();
                return 0;
            };";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void MultipleFunctionCallTest()
        {
            var source = @"
foo().bar();
foo().bar = 5;
(foo + 5).bar();
";
            ExecuteTest(source);
        }
       
        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
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

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestIntegerLiteralToken()
        {
            var source = "foo = 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestNegativeIntegerToken()
        {
            var source = "foo = -1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestStringLiteralToken()
        {
            var source = @"foo = 'bar';";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanLiteralTrue()
        {
            var source = @"foo = true;";
            ExecuteTest(source);
        }


        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBooleanLiteralFalse()
        {
            var source = @"foo = false;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestStringLiteralTokenDoubleQuote()
        {
            var source = @"foo = ""bar"";";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestFloatLiteralToken()
        {
            var source = "foo = 1.1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestWhileLoop()
        {
            string source =
          @"while(condition) function();
            while(condition()) {function();}";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestUnaryNegation()
        {
            var source = "foo = -bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestAddition()
        {
            var source = @"foo = 1 + bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBinaryAnd()
        {
            var source = @"foo = 1 & bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBinaryOr()
        {
            var source = @"foo = 1 | bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSubtraction()
        {
            var source = @"foo = bar - 1;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMultiplication()
        {
            var source = @"foo = bar * 1.5;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestDivision()
        {
            var source = @"foo = 1.5 / bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestAdditionAndMultiplicationOrder()
        {
            var source = @"foo = 5 + 1.2 * bar;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSubtractionAndDivisionOrder()
        {
            var source = @"foo = 1.5  - bar / 15;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSinglePrefix()
        {
            var source = "foo = ++i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestDoublePrefix()
        {
            var source = "foo = ++ ++i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMultiplePrefixes()
        {
            var source = "foo = --++~!++--i;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestSingleSuffix()
        {
            var source = "foo = i++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestDoubleSuffix()
        {
            var source = "foo = i++ ++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestMultipleSuffixes()
        {
            var source = "foo = i++--++--++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestBrackets()
        {
            var source = "foo = ((1 + 2) + 3 + bar(2 + 3));";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestPrefixSuffix()
        {
            var source = "foo = ++i++;";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestCodeSampleBottles()
        {
            var source = @"
auto Main = int()
{
	auto count = 99;
	System.Console.WriteLine(""{0} on the wall, {0}."", bottles(count));

	while ((count -= 1) >= 0)
	{
		System.Console.WriteLine(""Take one down and pass it around, {0} on the wall."", bottles(count));
		System.Console.WriteLine();
		System.Console.WriteLine(""{0} on the wall, {0}."", bottles(count));
	
	}
	System.Console.WriteLine(""Go to the store and buy some more, {0} on the wall."", bottles(99));
	System.Console.ReadKey();
	return 0;
};

auto bottles = string(int count)
{
	string ret;

	if (count > 0)
	{
		ret += count;

		if (count > 1)
		{
			ret += "" bottles "";
		}
		else
		{
			ret += "" bottle "";
		}
	}
	else
	{
		ret += ""no more bottles "";
	}

	ret += ""of beer"";
	return ret;
};
";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestCodeSampleRecursion()
        {
            var source = @"
auto IsEven = bool(int number)
{
    return number % 2 == 0;
};

auto Func = void(int i)
{
	if (IsEven(i))
	{
		System.Console.WriteLine(""{0} is even"", i);
		Func(i + 3);
	}
	else
	{
		System.Console.WriteLine(""{0} is odd"", i);
		Func(i + 5);
	}
};

auto Main = int()
{
	Func(0);
	return 0;
};
";
            ExecuteTest(source);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("SyntaxMatcher"), Timeout(timeout)]
        public void TestCodeSampleDeepRecursion()
        {
            var source = @"

auto Func = void(int i)
{
	auto Func = void(int i)
	{
		auto Func = void(int i)
		{
			auto Func = void(int i)
			{
				auto Func = void(int i)
				{
					auto Func = void(int i)
					{
						auto Func = void(int i)
						{
							auto Func = void(int i)
							{
								auto Func = void(int i)
								{
									auto Func = void(int i)
									{
										auto Func = void(int i)
										{
											auto Func = void(int i)
											{
												auto Func = void(int i)
												{
													auto Func = void(int i)
													{
														auto Func = void(int i)
														{
															auto Func = void(int i)
															{
																auto Func = void(int i)
																{
																	auto Func = void(int i)
																	{
																		auto Func = void(int i)
																		{
																			auto Func = void(int i)
																			{
																				auto Func = void(int i)
																				{
																					auto Func = void(int i)
																					{
																						System.Console.WriteLine(""{0} cycles were made."", i);
																						Func(i + 1);
																					};

																					Func(i);
																				};

																				Func(i);
																			};

																			Func(i);
																		};

																		Func(i);
																	};

																	Func(i);
																};
	
																Func(i);
															};

															Func(i);
														};

														Func(i);
													};

													Func(i);
												};

												Func(i);
											};

											Func(i);
										};

										Func(i);
									};

									Func(i);
								};

								Func(i);
							};

							Func(i);
						};

						Func(i);
					};

					Func(i);
				};

				Func(i);
			};

			Func(i);
		};

		Func(i);
	};

	Func(i);
};

auto Main = int()
{
	Func(0);
	return 0;
};

";
            ExecuteTest(source);
        }

        #endregion tests

        private void ExecuteTest(string source, [CallerMemberName] string fileName = "")
        {
            var tokenizedSource = Path + fileName + "_tokens.txt";
            var serializedTree = Path + fileName + "_tree.txt";

            using (var rootNode = new RootNode())
            {
                var tokens = Tokenizer.Tokenize(source, rootNode);

                string tree;

#if REMATCH
                tree = new SyntaxMatcher(tokens, rootNode).Match().ToString();
                System.IO.File.WriteAllText(serializedTree, tree);
#else
                tree = System.IO.File.ReadAllText(serializedTree);
#endif

                var syntaxMatcher = new SyntaxMatcher(tokens, rootNode);
                var actualTree = syntaxMatcher.Match();
                var actualTreeString = actualTree.ToString();
                Assert.AreEqual(tree, actualTreeString);
            }
        }
    }
}
