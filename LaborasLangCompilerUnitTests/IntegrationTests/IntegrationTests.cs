using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser;
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompilerUnitTests.IntegrationTests
{
    [TestClass]
    public class IntegrationTests : IntegrationTestBase
    {
        [TestMethod, TestCategory("Integration Tests")]
        public void Test_HelloWorld()
        {
            Test("HelloWorld.ll".Enumerate(), "Hello, world!" + Environment.NewLine);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_Bottles()
        {
            TestAgainstOutputInFile("Bottles");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_Recursion()
        {
            var expectedOutput =
@"0 is even
3 is odd
8 is even
";
            Test("Recursion.ll".Enumerate(), expectedOutput);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_StdInWorks()
        {
            var testInfo = new IntegrationTestInfo("StdInWorks.ll".Enumerate());

            testInfo.StdIn =
@"2
3
5
6
7
8
9
411
419
0
";

            testInfo.StdOut =
@"Enter 0 at any time to quit.
2 is a prime number
3 is a prime number
5 is a prime number
6 is not a prime number
7 is a prime number
8 is not a prime number
9 is not a prime number
411 is not a prime number
419 is a prime number
";

            Test(testInfo);
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_InlineFunctorCall()
        {
            Test("InlineFunctorCall.ll".Enumerate(), "It Works!");
        }

        [TestMethod, TestCategory("Integration Tests")]
        public void Test_MultipleFiles()
        {
            Test(new string[]{"InlineFunctorCall.ll", "MultipleFiles.ll"}, "It Works!");
        }
                
        [TestMethod, TestCategory("Integration Tests")]
        public void Test_PrintPrimesWithInlineLambda()
        {
            var testInfo = new IntegrationTestInfo("PrintPrimesWithInlineLambda.ll".Enumerate());

            testInfo.StdIn =
@"2
3
5
6
7
8
9
411
419
0
";

            testInfo.StdOut =
@"Enter 0 at any time to quit.
2 is a prime number
3 is a prime number
5 is a prime number
6 is not a prime number
7 is a prime number
8 is not a prime number
9 is not a prime number
411 is not a prime number
419 is a prime number
";

            Test(testInfo);

        }

        #region Helpers
        
        private string ExpectedOutputPath
        {
            get
            {
                return Path.Combine(IntegrationTestsPath, "ExpectedOutput");
            }
        }

        private void TestAgainstOutputInFile(string testName)
        {
            var expectedOutput = File.ReadAllText(Path.Combine(ExpectedOutputPath, testName) + ".txt");
            Test((testName + ".ll").Enumerate(), expectedOutput);
        }

        #endregion
    }
}
