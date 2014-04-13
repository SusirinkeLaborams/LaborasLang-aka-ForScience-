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
        public void TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral()
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
        public void TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral()
        {
            var property = new PropertyDefinition("doubleProperty", PropertyAttributes.HasDefault, assemblyRegistry.ImportType(typeof(double)));
            var backingField = new FieldDefinition("doubleProperty_backingField", FieldAttributes.Static, assemblyRegistry.ImportType(typeof(double)));

            typeEmitter.AddField(backingField);

            var setter = new MethodEmitter(assemblyRegistry, typeEmitter, "set_doubleProperty", assemblyRegistry.ImportType(typeof(void)),
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
                            LocalVariable = localVariable,
                        },
                        Initializer = new LiteralNode
                        {
                            ReturnType = assemblyRegistry.ImportType(typeof(double)),
                            Value = 5.5
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
                @"System.Void klass::set_doubleProperty(System.Double)",
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
                @"// Code size 17 (0x11)",
                @".maxstack 1",
                @".entrypoint",
                @".locals (",
                @"	[0] float64",
                @")",
                @"",
                @"IL_0000: ldc.r8 5.5",
                @"IL_0009: stloc.0",
                @"IL_000a: ldloc.0",
                @"IL_000b: call void klass::set_doubleProperty(float64)",
                @"IL_0010: ret"
            });

            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral()
        {
            var property = new PropertyDefinition("stringProperty", PropertyAttributes.HasDefault, assemblyRegistry.ImportType(typeof(string)));

            var getter = new MethodEmitter(assemblyRegistry, typeEmitter, "get_stringProperty", assemblyRegistry.ImportType(typeof(string)),
                MethodAttributes.Static | MethodAttributes.Private);

            getter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new LiteralNode()
                    {
                        ReturnType = assemblyRegistry.ImportType(typeof(string)),
                        Value = "Test"
                    }
                })
            });

            property.GetMethod = getter.Get();
            typeEmitter.AddProperty(property);

            var methodWithArgument = new MethodEmitter(assemblyRegistry, typeEmitter, "TestMethod", assemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = methodWithArgument.AddArgument(assemblyRegistry.ImportType(typeof(string)), "arg");

            methodWithArgument.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyRegistry.ImportType(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FunctionArgumentNode()
                            {
                                Param = argument
                            },
                            RightOperand = new PropertyNode()
                            {
                                Property = property
                            }
                        }
                    }
                })
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new FunctionCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Function = methodWithArgument.Get()
                        },
                        Arguments = new List<IExpressionNode>(new IExpressionNode[]
                        {
                            new LiteralNode()
                            {
                                ReturnType = assemblyRegistry.ImportType(typeof(string)),
                                Value = "Test"
                            }
                        })
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.String klass::get_stringProperty()",
                @"// Method begins at RVA 0x2050",
                @"// Code size 6 (0x6)",
                @".maxstack 8",
                @"",
                @"IL_0000: ldstr ""Test""",
                @"IL_0005: ret",
                @"",
                @"",
                @"System.Void klass::TestMethod(System.String)",
                @"// Method begins at RVA 0x2058",
                @"// Code size 8 (0x8)",
                @".maxstack 8",
                @"",
                @"IL_0000: call string klass::get_stringProperty()",
                @"IL_0005: starg.s arg",
                @"IL_0007: ret",
                @"",
                @"",
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2064",
                @"// Code size 11 (0xb)",
                @".maxstack 8",
                @".entrypoint",
                @"",
                @"IL_0000: ldstr ""Test""",
                @"IL_0005: call void klass::TestMethod(string)",
                @"IL_000a: ret"
            });

            Test();
        }

        [TestMethod]
        public void TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral()
        {
            var callableMethod = new MethodEmitter(assemblyRegistry, typeEmitter, "Test", assemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Private | MethodAttributes.Static);

            callableMethod.AddArgument(assemblyRegistry.ImportType(typeof(bool)), "isTrue");
            callableMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new FunctionCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Function = callableMethod.Get()
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode
                            {
                                ReturnType = assemblyRegistry.ImportType(typeof(bool)),
                                Value = true
                            }
                        }
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::Test(System.Boolean)",
                @"// Method begins at RVA 0x2050",
                @"// Code size 1 (0x1)",
                @".maxstack 8",
                @"",
                @"IL_0000: ret",
                @"",
                @"",
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2054",
                @"// Code size 7 (0x7)",
                @".maxstack 8",
                @".entrypoint",
                @"",
                @"IL_0000: ldc.i4.1",
                @"IL_0001: call void klass::Test(bool)",
                @"IL_0006: ret"
            });

            Test();
        }

        #endregion

        #region Operator tests

        [TestMethod]
        public void TestCanEmit_MultipleNestedAssignments()
        {
            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode()
                {
                    ReturnType = assemblyEmitter.ImportType(typeof(int)),
                    Value = 110
                }
            };

            const int count = 10;
            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Static | FieldAttributes.Private,
                    assemblyRegistry.ImportType(typeof(int)));
                typeEmitter.AddField(field);

                assignmentNode.LeftOperand = new FieldNode()
                {
                    Field = field
                };

                if (i != count - 1)
                {
                    var newNode = new AssignmentOperatorNode()
                    {
                        RightOperand = assignmentNode
                    };

                    assignmentNode = newNode;
                }
            }
            
            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyEmitter.ImportType(typeof(void)),
                        Operand = assignmentNode
                    }
                })
            };

            ExpectedIL = string.Join("\r\n", new string[]
            {
                @"System.Void klass::dummy()",
                @"// Method begins at RVA 0x2050",
                @"// Code size 62 (0x3e)",
                @".maxstack 8",
                @".entrypoint",
                @"",
                @"IL_0000: ldc.i4.s 110",
                @"IL_0002: dup",
                @"IL_0003: stsfld int32 klass::intField0",
                @"IL_0008: dup",
                @"IL_0009: stsfld int32 klass::intField1",
                @"IL_000e: dup",
                @"IL_000f: stsfld int32 klass::intField2",
                @"IL_0014: dup",
                @"IL_0015: stsfld int32 klass::intField3",
                @"IL_001a: dup",
                @"IL_001b: stsfld int32 klass::intField4",
                @"IL_0020: dup",
                @"IL_0021: stsfld int32 klass::intField5",
                @"IL_0026: dup",
                @"IL_0027: stsfld int32 klass::intField6",
                @"IL_002c: dup",
                @"IL_002d: stsfld int32 klass::intField7",
                @"IL_0032: dup",
                @"IL_0033: stsfld int32 klass::intField8",
                @"IL_0038: stsfld int32 klass::intField9",
                @"IL_003d: ret"
            });

            Test();
        }

        #endregion

        #region Functor tests

        [TestMethod]
        public void TestCanEmit_FunctorDefinition()
        {
            FunctorTypeEmitter.Create(assemblyRegistry, assemblyEmitter, assemblyRegistry.ImportType(typeof(void)), new List<TypeReference>());

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

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
