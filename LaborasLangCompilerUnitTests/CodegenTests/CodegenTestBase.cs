using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    public class CodegenTestBase : TestBase
    {
        protected string ExpectedILFilePath { get; set; }
        internal protected string ExpectedOutput { get; set; }
        internal ICodeBlockNode BodyCodeBlock { get; set; }

        internal MethodEmitter methodEmitter { get; private set; }
        internal TypeEmitter typeEmitter { get; private set; }
        internal AssemblyEmitter assemblyEmitter { get; private set; }

        internal const string kEntryPointMethodName = "ExecuteTest";
        private readonly bool bulkTesting = false;

        private readonly MethodReference consoleWrite;
        private readonly MethodReference consoleWriteLine;
        private readonly MethodReference consoleWriteLineParams;

        private readonly MethodReference ienumerableGetEnumerator;

        private readonly TypeReference ienumerator;
        private readonly MethodReference ienumeratorMoveNext;
        private readonly MethodReference ienumeratorGetCurrent;

        public CodegenTestBase() :
            this(null, "Class", false)
        {
        }

        internal static AssemblyEmitter CreateTempAssembly()
        {
            var tempLocation = Path.Combine(GetTestDirectory(), "TestExecutable.exe");
            var compilerArgs = CompilerArguments.Parse(new[] { "ExecuteTest.il", "/out:" + tempLocation });
            return new AssemblyEmitter(compilerArgs);
        }

        internal CodegenTestBase(AssemblyEmitter assembly, string className, bool bulkTesting) :
            base(!bulkTesting)
        {
            assemblyEmitter = assembly ?? CreateTempAssembly();
            typeEmitter = new TypeEmitter(assemblyEmitter, className);
            methodEmitter = new MethodEmitter(typeEmitter, kEntryPointMethodName, assemblyEmitter.TypeSystem.Void,
                MethodAttributes.Static | MethodAttributes.Assembly);

            consoleWrite = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "Write",
                new[] { assemblyEmitter.TypeSystem.Object });

            consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine",
                new[] { assemblyEmitter.TypeSystem.Object });
            
            consoleWriteLineParams = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine",
                new[] { assemblyEmitter.TypeSystem.String, new ArrayType(assemblyEmitter.TypeSystem.Object) });

            ienumerableGetEnumerator = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Collections.IEnumerable",
                "GetEnumerator", new TypeReference[0]);

            ienumerator = AssemblyRegistry.FindType(assemblyEmitter, "System.Collections.IEnumerator");

            ienumeratorMoveNext = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, ienumerator,
                "MoveNext", new TypeReference[0]);

            var ienumerableCurrent = AssemblyRegistry.GetProperty(assemblyEmitter, ienumerator, "Current");
            ienumeratorGetCurrent = AssemblyRegistry.GetPropertyGetter(assemblyEmitter, ienumerableCurrent);

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

            PEVerifyRunner.Run(assemblyEmitter.OutputPath);
            var stdout = ManagedCodeRunner.CreateProcessAndRun(assemblyEmitter.OutputPath, new string[0] { });
            Assert.AreEqual(ExpectedOutput.Trim(), stdout.Trim());
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

        internal ICodeBlockNode OutputEnumerable(IExpressionNode enumerable)
        {
            var enumerator = new LocalVariableNode(new VariableDefinition(ienumerator));
            var counter = new LocalVariableNode(new VariableDefinition(assemblyEmitter.TypeSystem.Int32));

            return new CodeBlockNode()
            {
                Nodes = new IParserNode[] 
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = counter.LocalVariable,
                        Initializer = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 0)
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = enumerator.LocalVariable,
                        Initializer = new MethodCallNode()
                        {
                            ExpressionReturnType = ienumerator,
                            Function = new FunctionNode()
                            {
                                ObjectInstance = enumerable,
                                Method = ienumerableGetEnumerator
                            },
                            Args = new IExpressionNode[0]
                        }
                    },
                    new WhileBlockNode()
                    {
                        Condition = new MethodCallNode()
                        {
                            Function = new FunctionNode()
                            {
                                ObjectInstance = enumerator,
                                Method = ienumeratorMoveNext
                            },
                            Args = new IExpressionNode[0]
                        },
                        ExecutedBlock = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                new MethodCallNode()
                                {
                                    Function = new FunctionNode()
                                    {
                                        Method = consoleWrite
                                    },
                                    Args = new IExpressionNode[]
                                    {
                                        new BinaryOperatorNode()
                                        {
                                            ExpressionReturnType = assemblyEmitter.TypeSystem.String,
                                            BinaryOperatorType = BinaryOperatorNodeType.Addition,
                                            LeftOperand = new MethodCallNode()
                                            {
                                                ExpressionReturnType = assemblyEmitter.TypeSystem.Object,
                                                Function = new FunctionNode()
                                                {
                                                    ObjectInstance = enumerator,
                                                    Method = ienumeratorGetCurrent
                                                },
                                                Args = new IExpressionNode[0]
                                            },
                                            RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.String, " ")
                                        }
                                    }
                                },
                                new UnaryOperatorNode()
                                {
                                    UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                                    ExpressionReturnType = assemblyEmitter.TypeSystem.Void,
                                    Operand = new IncrementDecrementOperatorNode()
                                    {
                                        IncrementDecrementType = IncrementDecrementOperatorType.PreIncrement,
                                        ExpressionReturnType = assemblyEmitter.TypeSystem.Int32,
                                        Operand = counter
                                    }
                                }
                            }
                        }
                    },
                    CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeSystem.String, "")),
                    CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeSystem.String, "Total count: {0}."), counter)
                }
            };
        }

        internal MethodReference EmitMethodToOutputArgs(IExpressionNode returnValue, params TypeReference[] args)
        {
            var returnType = returnValue != null ? returnValue.ExpressionReturnType : assemblyEmitter.TypeSystem.Void;
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
            
                consoleWriteLineArgs.Add(new LiteralNode(assemblyEmitter.TypeSystem.String, formatString.ToString()));
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
