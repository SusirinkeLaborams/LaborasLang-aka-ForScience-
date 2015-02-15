using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    public class ILTestBase : TestBase
    {
        protected string ExpectedILFilePath { get; set; }
        internal protected string ExpectedOutput { get; set; }
        internal ICodeBlockNode BodyCodeBlock { get; set; }

        internal MethodEmitter methodEmitter { get; private set; }
        internal TypeEmitter typeEmitter { get; private set; }
        internal AssemblyEmitter assemblyEmitter { get; private set; }

        internal const string kEntryPointMethodName = "dummy";
        private readonly MethodReference consoleWriteLine;
        private readonly MethodReference consoleWriteLineParams;
        private readonly bool bulkTesting = false;

        public ILTestBase() :
            this(CreateTempAssembly(), "Class", false)
        {
        }

        internal static AssemblyEmitter CreateTempAssembly()
        {
            var tempLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            var compilerArgs = CompilerArguments.Parse(new[] { "dummy.il", "/out:" + tempLocation });
            return new AssemblyEmitter(compilerArgs);
        }

        internal ILTestBase(AssemblyEmitter assembly, string className, bool bulkTesting) :
            base(!bulkTesting)
        {
            assemblyEmitter = assembly;
            typeEmitter = new TypeEmitter(assemblyEmitter, className);
            methodEmitter = new MethodEmitter(typeEmitter, kEntryPointMethodName, assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Assembly);

            consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine",
                new[] { assemblyEmitter.TypeToTypeReference(typeof(object)) });
            consoleWriteLineParams = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine",
                new[] { assemblyEmitter.TypeToTypeReference(typeof(string)), assemblyEmitter.TypeToTypeReference(typeof(object[])) });

            this.bulkTesting = bulkTesting;
        }

        protected void AssertSuccessByILComparison()
        {
            if (BodyCodeBlock == null)
            {
                BodyCodeBlock = new CodeBlockNode()
                {
                    Nodes = new List<IParserNode>()
                };
            }

            methodEmitter.ParseTree(BodyCodeBlock);
            methodEmitter.SetAsEntryPoint();
            assemblyEmitter.Save();

            var il = Disassembler.DisassembleAssembly(assemblyEmitter.OutputPath);

            var expectedILPath = Path.Combine("..", "..", "CodegenTests", "MethodBodyTests", "Expected", ExpectedILFilePath);
            var expectedIL = File.ReadAllText(expectedILPath, Encoding.UTF8);

            try
            {
                Assert.AreEqual(expectedIL.Trim(), il.Trim());
                PEVerifyRunner.Run(assemblyEmitter.OutputPath);
            }
            finally
            {
                File.Delete(assemblyEmitter.OutputPath);
                File.Delete(Path.ChangeExtension(assemblyEmitter.OutputPath, ".pdb"));
            }
        }

        protected void AssertSuccessByExecution()
        {
            if (!bulkTesting)
            {
                AssertSuccessByExecutionForSingleTest();
            }
        }

        private void AssertSuccessByExecutionForSingleTest()
        {
            Assert.IsNotNull(BodyCodeBlock);

            methodEmitter.ParseTree(BodyCodeBlock);
            methodEmitter.SetAsEntryPoint();
            assemblyEmitter.Save();

            try
            {
                PEVerifyRunner.Run(assemblyEmitter.OutputPath);
                var stdout = ManagedCodeRunner.CreateProcessAndRun(assemblyEmitter.OutputPath, new string[0] { });
                Assert.AreEqual(ExpectedOutput.Trim(), stdout.Trim());
            }
            finally
            {
                try
                {   // Deleting PDB will fail if we're debugging, 
                    // since we're executing the code in the same process, so VS will have it loaded
                    File.Delete(assemblyEmitter.OutputPath);
                    File.Delete(Path.ChangeExtension(assemblyEmitter.OutputPath, ".pdb"));
                }
                catch
                {
                }
            }
        }

        internal void GenerateBodyToOutputExpression(IExpressionNode expression)
        {
            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(expression)
                }
            };
        }

        internal IParserNode CallConsoleWriteLine(IExpressionNode expression)
        {
            return new MethodCallNode()
            {
                Function = new FunctionNode()
                {
                    Method = consoleWriteLine
                },
                Args = new List<IExpressionNode>()
                {
                    expression
                }
            };
        }
        internal IParserNode CallConsoleWriteLine(params IExpressionNode[] args)
        {
            return new MethodCallNode()
            {
                Function = new FunctionNode()
                {
                    Method = consoleWriteLineParams
                },
                Args = new List<IExpressionNode>(args)
            };
        }

        internal MethodReference EmitMethodToOutputArgs(IExpressionNode returnValue, params TypeReference[] args)
        {
            var returnType = returnValue != null ? returnValue.ExpressionReturnType : assemblyEmitter.TypeToTypeReference(typeof(void));
            var targetMethod = new MethodEmitter(typeEmitter, "TargetMethod", returnType, MethodAttributes.Static | MethodAttributes.Private);

            var nodes = new List<IParserNode>();

            if (args.Length > 0)
            {
                var consoleWriteLineArgs = new List<IExpressionNode>();
                var parameters = args.Select(a => targetMethod.AddArgument(new ParameterDefinition(a))).ToArray();
                var formatString = new StringBuilder();

                for (int i = 0; i < parameters.Length; i++)
                {
                    formatString.Append(string.Format("{{{0}}}", i));

                    if (i != parameters.Length - 1)
                        formatString.Append("\r\n");
                }
            
                consoleWriteLineArgs.Add(new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(string)), formatString.ToString()));
                consoleWriteLineArgs.AddRange(parameters.Select(p => new ParameterNode(p)));

                nodes.Add(CallConsoleWriteLine(consoleWriteLineArgs.ToArray()));
            }

            if (returnValue != null)
            {
                nodes.Add(new ReturnNode()
                {
                    Expression = returnValue
                });
            }

            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = nodes
            });

            return targetMethod.Get();
        }
    }
}
