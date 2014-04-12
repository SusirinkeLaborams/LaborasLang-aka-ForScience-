using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Misc;
using LaborasLangCompiler.Parser.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    public class MethodBodyTests
    {
        private string ExpectedIL { get; set; }
        private ICodeBlockNode BodyCodeBlock { get; set; }

        private CompilerArguments compilerArgs;
        private AssemblyRegistry assemblyRegistry;

        [TestMethod]
        public void TestCanEmitEmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"// Method begins at RVA 0x2050",
                @"// Code size 1 (0x1)",
                @".maxstack 8",
                @".entrypoint",
                @"",
                @"IL_0000: ret"
            });

            Test();
        }

        [TestMethod]
        public void TestCanEmitHelloWorld()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new FunctionCallNode()
                    {
                        ReturnType = assemblyRegistry.ImportType(typeof(void)),
                        Function = new FunctionNode()
                        {
                            Function = assemblyRegistry.GetMethods("System.Console", "WriteLine")
                                            .Single(x => x.Parameters.Count == 1 && x.Parameters[0].ParameterType.FullName == "System.String")
                        },
                        Arguments = new List<IExpressionNode>(new IExpressionNode[]
                        {
                            new LiteralNode()
                            {
                                ReturnType = assemblyRegistry.ImportType(typeof(string)),
                                Value = "Hello, world!"
                            }
                        })
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyRegistry.ImportType(typeof(void)),
                        Operand = new FunctionCallNode()
                        {
                            ReturnType = assemblyRegistry.ImportType(typeof(ConsoleKeyInfo)),
                            Function = new FunctionNode()
                            {
                                Function = assemblyRegistry.GetMethods("System.Console", "ReadKey").Single(x => x.Parameters.Count == 0)
                            },
                            Arguments = new List<IExpressionNode>(new IExpressionNode[]
                            {
                            })
                        }
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"// Method begins at RVA 0x2050",
                @"// Code size 17 (0x11)",
                @".maxstack 8",
                @".entrypoint",
                @"",
                @"IL_0000: ldstr ""Hello, world!""",
                @"IL_0005: call void [mscorlib]System.Console::WriteLine(string)",
                @"IL_000a: call valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()",
                @"IL_000f: pop",
                @"IL_0010: ret"
            });

            Test();
        }

        #region Helpers

        public MethodBodyTests()
        {
            compilerArgs = CompilerArguments.Parse(new[] { "dummy.il" });
            assemblyRegistry = new AssemblyRegistry(compilerArgs.References);
        }

        private void Test()
        {
            var assemblyEmitter = new AssemblyEmitter(compilerArgs);
            var typeEmitter = new TypeEmitter(assemblyEmitter, "klass");
            var methodEmitter = new MethodEmitter(assemblyRegistry, typeEmitter, "dummy", assemblyRegistry.ImportType(typeof(void)), 
                MethodAttributes.Static | MethodAttributes.Private);

            methodEmitter.ParseTree(BodyCodeBlock);
            methodEmitter.SetAsEntryPoint();
            assemblyEmitter.Save();
            
            var il = Disassembler.DisassembleAssembly(assemblyEmitter.OutputPath);

            try
            {
                Assert.AreEqual(ExpectedIL, il.Trim());
            }
            finally
            {
                File.Delete(assemblyEmitter.OutputPath);
            }
        }

        #endregion
    }
}
