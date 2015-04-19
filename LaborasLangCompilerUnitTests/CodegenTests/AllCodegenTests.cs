using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    [TestClass]
    public class AllCodegenTests
    {
        [TestMethod, TestCategory("All Tests")]
        public void AllExecutionBasedCodegenTests()
        {
            var baseType = typeof(CodegenTestBase);
            var allMethods = baseType.Assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t)).SelectMany(t => t.GetMethods());
            var testMethods = new List<MethodInfo>();

            foreach (var method in allMethods)
            {
                var customAttributes = method.GetCustomAttributes(typeof(TestCategoryAttribute), false);

                foreach (TestCategoryAttribute attribute in customAttributes)
                {
                    if (attribute.TestCategories.Contains("Execution Based Codegen Tests"))
                    {
                        testMethods.Add(method);
                        break;
                    }
                }
            }

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
                var startTestMessage = string.Format("Starting test: {0}", testMethods[i].Name);
                var messageIndex = stdOut.IndexOf(startTestMessage);

                if (messageIndex == -1)
                {
                    testReport.AppendLine(string.Format("Test {0} didn't run.", testMethods[i].Name));
                    didntRunTests++;
                    continue;
                }

                var messageEndIndex = stdOut.IndexOf("Starting test:", messageIndex + startTestMessage.Length + 1);
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

        [TestMethod, TestCategory("All Tests")]
        public void AllMiscILTests()
        {
            var baseType = typeof(TestBase);
            var allMethods = baseType.Assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t)).SelectMany(t => t.GetMethods());
            var testMethods = new List<MethodInfo>();

            foreach (var method in allMethods)
            {
                var customAttributes = method.GetCustomAttributes(typeof(TestCategoryAttribute), false);

                foreach (TestCategoryAttribute attribute in customAttributes)
                {
                    if (attribute.TestCategories.Contains("Misc IL Tests"))
                    {
                        testMethods.Add(method);
                        break;
                    }
                }
            }

            foreach (var method in testMethods)
            {
                var instance = Activator.CreateInstance(method.DeclaringType);
                method.Invoke(instance, new object[0]);
            }
        }
    }
}
