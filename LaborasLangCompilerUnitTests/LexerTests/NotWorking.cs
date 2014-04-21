using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LaborasLangCompilerUnitTests.ILTests;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class NotWorking : TestBase
    {
        [TestMethod]
        public void TestCanLex_FunctionThatReturnsFunction()
        {
            var source =
@"
auto Func = int()()
{
	return int()
	{
		System.Console.WriteLine(""Func inner called"");
		return 5;
	}
};

auto Main = int()
{
	System.Console.WriteLine(Func()());
	System.Console.ReadKey();
	return 0;
};";

            var tree = lexer.MakeTree(source);
            Assert.IsTrue(tree != null && tree.Children.Count > 0);
        }

        [TestMethod]
        public void TestCanLex_ConditionFromReturnedMethod()
        {
            var source =
@"auto Main = int()
{
	System.Console.WriteLine(""Please enter your name: "");
	auto name = System.Console.ReadLine();

	if (System.Char.IsLower(name, 0))
	{
		System.Console.WriteLine(""Does your name really start with lower case letter? LOLOLOLOL"");
	}
	else
	{
		System.Console.WriteLine(""Hi, {0}! It is {1}."", name, System.DateTime.Now);
	}

	System.Console.ReadKey();
	return 0;
};";

            var tree = lexer.MakeTree(source);
            Assert.IsTrue(tree != null && tree.Children.Count > 0);
        }

        [TestMethod]
        public void TestCanLex_WhileLoopAndPostIncrementOperator()
        {
            var source =
@"auto DoComplexMath = double(double argument)
{
	auto counter;

	while (argument < 20 && counter < 10000)
	{
		argument = 15 * argument - 20 * 10;
		counter++;
	}

	return argument;
};

auto Main = int()
{
	System.Console.WriteLine(DoComplexMath(12.1111));
	System.Console.ReadKey();
	return 0;
};";

            var tree = lexer.MakeTree(source);
            Assert.IsTrue(tree != null && tree.Children.Count > 0);
        }

        [TestMethod]
        public void TestCanLex_ShiftOperator()
        {
            var source =
@"auto Main = int()
{
	System.Console.WriteLine(50 >> 3);
	System.Console.ReadKey();
	return 0;
};";

            var tree = lexer.MakeTree(source);
            Assert.IsTrue(tree != null && tree.Children.Count > 0);
        }
    }
}
