using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Parser;
using LaborasLangCompilerUnitTests.CodegenTests;
using LaborasLangCompilerUnitTests.LexerTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BindingFlags = System.Reflection.BindingFlags;
using MethodInfo = System.Reflection.MethodInfo;

namespace LaborasLangCompilerUnitTests
{
    [TestClass]
    public class CombinedTests : TestBase
    {
        public CombinedTests() :
            base(false)
        {
        }

        [TestMethod, TestCategory("All Tests")]
        public void AllTests()
        {
            var testMethods = GetAllTestsFromCategory("Combined Test Suites");

            foreach (var method in testMethods)
            {
                var instance = Activator.CreateInstance(method.DeclaringType);
                method.Invoke(instance, new object[0]);
            }
        }

        [TestMethod, TestCategory("Combined Test Suites")]
        public void AllIntegrationTests()
        {
            var testMethods = GetAllTestsFromCategory("Integration Tests");

            foreach (var method in testMethods)
            {
                var instance = Activator.CreateInstance(method.DeclaringType);
                method.Invoke(instance, new object[0]);
            }
        }

        [TestMethod, TestCategory("Combined Test Suites")]
        public void TestParser()
        {
            var testClass = typeof(LaborasLangCompilerUnitTests.ParserTests.ParserTests);
            var methods = testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Any()).ToList();
            var instance = new LaborasLangCompilerUnitTests.ParserTests.ParserTests();

            foreach (var method in methods)
                method.Invoke(instance, null);
        }

        [TestMethod, TestCategory("Combined Test Suites")]
        public void TestLexer()
        {
            var classes = new List<Type>() { typeof(RuleValidation), typeof(SyntaxMatcherTests), typeof(TokenizerTests), typeof(ValueBlockTests) };

            foreach (var cls in classes)
            {
                var methods = cls.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Any());
                var instance = cls.GetConstructor(new Type[] { }).Invoke(null);

                foreach (var method in methods)
                    method.Invoke(instance, null);
            }
        }

        [TestMethod, TestCategory("Combined Test Suites")]
        public void AllMiscILTests()
        {
            var testMethods = GetAllTestsFromCategory("Misc IL Tests").GroupBy(m => m.DeclaringType);

            foreach (var methodGroup in testMethods)
            {
                var instance = Activator.CreateInstance(methodGroup.Key);

                foreach (var method in methodGroup)
                    method.Invoke(instance, new object[0]);
            }
        }

        [TestMethod, TestCategory("Combined Test Suites")]
        public void AllExecutionBasedCodegenTests()
        {
            var testMethods = GetAllTestsFromCategory("Execution Based Codegen Tests");

            var tests = new List<CodegenTestBase>();
            var compilerArgs = CompilerArguments.Parse(new[] { "ExecuteTest.il" });
            AssemblyRegistry.CreateAndOverrideIfNeeded(compilerArgs.References);
            var assembly = CodegenTestBase.CreateTempAssembly();

            foreach (var method in testMethods)
            {
                var ctor = method.DeclaringType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { typeof(AssemblyEmitter), typeof(string), typeof(bool) }, null);
                var instance = (CodegenTestBase)ctor.Invoke(new object[] { assembly, method.Name, true });
                method.Invoke(instance, new object[] { });
                instance.methodEmitter.ParseTree(instance.BodyCodeBlock);
                tests.Add(instance);
            }

            EmitEntryPoint(assembly, testMethods);
            assembly.Save();

            ExecuteTests(assembly.OutputPath, testMethods, tests);
        }

        private void EmitEntryPoint(AssemblyEmitter assembly, IEnumerable<MethodInfo> testMethods)
        {
            var voidType = assembly.TypeSystem.Void;
            var stringType = assembly.TypeSystem.String;

            var entryPointType = new TypeEmitter(assembly, "TestEntryPointClass");
            var entryPointMethod = new MethodEmitter(entryPointType, "EntryPoint", voidType, MethodAttributes.Private | MethodAttributes.Static);
            entryPointMethod.SetAsEntryPoint();

            var nodes = new List<IParserNode>();
            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assembly, "System.Console", "WriteLine", new TypeReference[] { stringType });

            foreach (var method in testMethods)
            {
                nodes.Add(new MethodCallNode()
                {
                    Function = new FunctionNode()
                    {
                        Method = consoleWriteLine
                    },
                    Args = new IExpressionNode[]
                    {
                        new LiteralNode(stringType, string.Format("Starting test: {0}", method.Name))
                    }
                });

                nodes.Add(new MethodCallNode()
                {
                    Function = new FunctionNode()
                    {
                        Method = AssemblyRegistry.GetMethod(assembly, method.Name, CodegenTestBase.kEntryPointMethodName)
                    },
                    Args = new IExpressionNode[0]
                });
            }

            entryPointMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = nodes
            });
        }

        private void ExecuteTests(string executablePath, IReadOnlyList<MethodInfo> testMethods, IReadOnlyList<CodegenTestBase> tests)
        {
            PEVerifyRunner.Run(executablePath);
            bool ranSuccessfully = false;
            string stdOut = string.Empty;

            try
            {
                stdOut = ManagedCodeRunner.CreateProcessAndRun(executablePath, new string[0]);
                ranSuccessfully = true;
            }
            catch (Exception e)
            {
                stdOut = e.Message;
            }

            int succeededTests = 0,
                failedTests = 0,
                didntRunTests = 0;

            var testReport = new StringBuilder();

            for (int i = 0; i < testMethods.Count; i++)
            {
                var startTestMessage = string.Format("Starting test: {0}\r\n", testMethods[i].Name);
                var messageIndex = stdOut.IndexOf(startTestMessage);

                if (messageIndex == -1)
                {
                    testReport.AppendLine(string.Format("Test {0} didn't run.", testMethods[i].Name));
                    didntRunTests++;
                    continue;
                }

                var messageEndIndex = stdOut.IndexOf("Starting test:", messageIndex + startTestMessage.Length);
                string testOutput;

                if (messageEndIndex != -1)
                {
                    testOutput = stdOut.Substring(messageIndex + startTestMessage.Length, messageEndIndex - messageIndex - startTestMessage.Length).Trim();
                }
                else
                {
                    testOutput = stdOut.Substring(messageIndex + startTestMessage.Length).Trim();
                }

                if (testOutput != tests[i].ExpectedOutput.Trim())
                {
                    failedTests++;
                    testReport.AppendLine(string.Format("Test {0} failed.\r\nExpected output:\r\n{1}\r\nActual output:\r\n{2}\r\n", testMethods[i].Name, tests[i].ExpectedOutput, testOutput));
                }
                else
                {
                    succeededTests++;
                }
            }

            if (ranSuccessfully && failedTests == 0 && didntRunTests == 0)
            {
                Assert.IsTrue(true);
                return;
            }

            throw new Exception(string.Format("Tests failed!\r\nSucceeded tests: {0}\r\nFailed tests: {1}\r\nTests that didn't run: {2}\r\n{3}",
                succeededTests, failedTests, didntRunTests, testReport.ToString()));
        }

        private static IReadOnlyList<MethodInfo> GetAllTestsFromCategory(string category)
        {
            var allMethods = typeof(TestBase).Assembly.GetTypes().SelectMany(t => t.GetMethods());
            var testMethods = new List<MethodInfo>();

            foreach (var method in allMethods)
            {
                var customAttributes = method.GetCustomAttributes(typeof(TestCategoryAttribute), false);
                bool hasCategory = false;
                bool isDisabled = false;

                foreach (TestCategoryAttribute attribute in customAttributes)
                {
                    if (attribute.TestCategories.Contains(category))
                    {
                        hasCategory = true;
                    }
                    else if (attribute.TestCategories.Contains("Disabled") || attribute.TestCategories.Contains("CodeSamples"))
                    {
                        isDisabled = true;
                    }
                }
                
                if (hasCategory && !isDisabled)
                    testMethods.Add(method);
            }

            return testMethods;
        }
    }
}
