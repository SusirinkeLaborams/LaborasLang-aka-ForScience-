using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    public class ILTestBase : TestBase
    {
        protected string ExpectedILFilePath { get; set; }
        protected string ExpectedOutput { get; set; }
        internal ICodeBlockNode BodyCodeBlock { get; set; }

        internal MethodEmitter methodEmitter { get; private set; }
        internal TypeEmitter typeEmitter { get; private set; }
        internal AssemblyEmitter assemblyEmitter { get; private set; }

        private readonly CompilerArguments compilerArgs;
        private readonly MethodReference consoleWriteLine;
        private readonly MethodReference consoleWriteLineParams;

        public ILTestBase()
        {
            var tempLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            compilerArgs = CompilerArguments.Parse(new[] { "dummy.il", "/out:" + tempLocation });
            assemblyEmitter = new AssemblyEmitter(compilerArgs);
            typeEmitter = new TypeEmitter(assemblyEmitter, "klass");
            methodEmitter = new MethodEmitter(typeEmitter, "dummy", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine",
                new[] { assemblyEmitter.TypeToTypeReference(typeof(object)) });
            consoleWriteLineParams = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine",
                new[] { assemblyEmitter.TypeToTypeReference(typeof(string)), assemblyEmitter.TypeToTypeReference(typeof(object[])) });
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
    }
}
