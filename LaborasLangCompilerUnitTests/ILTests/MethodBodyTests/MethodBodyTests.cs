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
using Mono.Cecil.Cil;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    public class MethodBodyTests
    {
        private string ExpectedIL { get; set; }
        private ICodeBlockNode BodyCodeBlock { get; set; }

        private CompilerArguments compilerArgs;
        private AssemblyRegistry assemblyRegistry;
        private TypeEmitter typeEmitter;
        private AssemblyEmitter assemblyEmitter;

        #region Test methods

        [TestMethod]
        public void TestCanEmitEmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::dummy()",
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
                @"System.Void klass::dummy()",
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

        #region Load/Store lvalues and load literals tests

        [TestMethod]
        public void TestCanEmit_VariableDeclarationAndInitialization_LoadLiteralFloat()
        {
            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("floatValue", assemblyRegistry.ImportType(typeof(float)))                            
                        },
                        Initializer = new LiteralNode()
                        {
                            ReturnType = assemblyRegistry.ImportType(typeof(float)),
                            Value = 2.5
                        }
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2050",
                @"// Code size 7 (0x7)",
                @".maxstack 1",
                @".entrypoint",
                @".locals (",
                @"	[0] float32",
                @")",
                @"",
                @"IL_0000: ldc.r4 2.5",
                @"IL_0005: stloc.0",
                @"IL_0006: ret"
            });
            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreField_LoadIntLiteral()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyRegistry.ImportType(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyRegistry.ImportType(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new LiteralNode()
                            {
                                ReturnType = assemblyRegistry.ImportType(typeof(int)),
                                Value = 1
                            }
                        }
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2050",
                @"// Code size 7 (0x7)",
                @".maxstack 8",
                @".entrypoint",
                @"",
                @"IL_0000: ldc.i4.1",
                @"IL_0001: stsfld int32 klass::intField",
                @"IL_0006: ret"
            });

            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreLocalVariable_LoadField()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyRegistry.ImportType(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("intLocal", assemblyRegistry.ImportType(typeof(int)))
                        },
                        Initializer = new FieldNode()
                        {
                            Field = field
                        }
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2050",
                @"// Code size 7 (0x7)",
                @".maxstack 1",
                @".entrypoint",
                @".locals (",
                @"	[0] int32",
                @")",
                @"",
                @"IL_0000: ldsfld int32 klass::intField",
                @"IL_0005: stloc.0",
                @"IL_0006: ret"
            });

            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument()
        {
            var property = new PropertyDefinition("doubleProperty", PropertyAttributes.HasDefault, assemblyRegistry.ImportType(typeof(double)));
            var backingField = new FieldDefinition("doubleProperty_backingField", FieldAttributes.Static, assemblyRegistry.ImportType(typeof(double)));

            typeEmitter.AddField(backingField);

            var setter = new MethodEmitter(assemblyRegistry, typeEmitter, "set_doubleField", assemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = setter.AddArgument(assemblyRegistry.ImportType(typeof(double)), "value");
            setter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyRegistry.ImportType(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = backingField
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                Param = argument
                            }
                        }
                    }
                })
            });

            property.SetMethod = setter.Get();

            typeEmitter.AddProperty(property);

            var localVariable = new VariableDefinition("doubleLocal", assemblyRegistry.ImportType(typeof(double)));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyRegistry.ImportType(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new PropertyNode()
                            {
                                Property = property
                            },
                            RightOperand = new LocalVariableNode()
                            {
                                LocalVariable = localVariable
                            }
                        }
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::set_doubleField(System.Double)",
                @"// Method begins at RVA 0x2050",
                @"// Code size 7 (0x7)",
                @".maxstack 8",
                @"",
                @"IL_0000: ldarg.0",
                @"IL_0001: stsfld float64 klass::doubleProperty_backingField",
                @"IL_0006: ret",
                @"",
                @"",
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2058",
                @"// Code size 7 (0x7)",
                @".maxstack 1",
                @".entrypoint",
                @".locals (",
                @"	[0] float64",
                @")",
                @"",
                @"IL_0000: ldloc.0",
                @"IL_0001: call void klass::set_doubleField(float64)",
                @"IL_0006: ret"
            });

            Test();
        }
        
        #endregion

        #endregion

        #region Helpers

        public MethodBodyTests()
        {
            var tempLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            compilerArgs = CompilerArguments.Parse(new[] { "dummy.il", "/out:" + tempLocation });
            assemblyRegistry = new AssemblyRegistry(compilerArgs.References);
            assemblyEmitter = new AssemblyEmitter(compilerArgs);
            typeEmitter = new TypeEmitter(assemblyEmitter, "klass");
        }

        private void Test()
        {
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
                File.Delete(Path.ChangeExtension(assemblyEmitter.OutputPath, ".pdb"));
            }
        }

        #endregion
    }
}
