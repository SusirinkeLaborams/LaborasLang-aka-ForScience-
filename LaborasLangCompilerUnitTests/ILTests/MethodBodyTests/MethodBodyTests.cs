using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Misc;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.ILTools.Methods;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    public class MethodBodyTests : TestBase
    {
        private string ExpectedILFilePath { get; set; }
        private ICodeBlockNode BodyCodeBlock { get; set; }

        private CompilerArguments compilerArgs;
        private TypeEmitter typeEmitter;
        private AssemblyEmitter assemblyEmitter;

        #region Test methods

        [TestMethod]
        public void TestCanEmit_EmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_EmptyMethod.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_HelloWorld()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Function = AssemblyRegistry.GetMethods("System.Console", "WriteLine")
                                            .Single(x => x.Parameters.Count == 1 && x.Parameters[0].ParameterType.FullName == "System.String")
                        },
                        Arguments = new List<IExpressionNode>(new IExpressionNode[]
                        {
                            new LiteralNode()
                            {
                                ReturnType = AssemblyRegistry.ImportType(typeof(string)),
                                Value = "Hello, world!"
                            }
                        })
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = AssemblyRegistry.ImportType(typeof(void)),
                        Operand = new MethodCallNode()
                        {
                            Function = new FunctionNode()
                            {
                                Function = AssemblyRegistry.GetMethods("System.Console", "ReadKey").Single(x => x.Parameters.Count == 0)
                            },
                            Arguments = new List<IExpressionNode>(new IExpressionNode[]
                            {
                            })
                        }
                    }
                })
            };

            ExpectedILFilePath = "TestCanEmit_HelloWorld.il";
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
                            LocalVariable = new VariableDefinition("floatValue", AssemblyRegistry.ImportType(typeof(float)))                            
                        },
                        Initializer = new LiteralNode()
                        {
                            ReturnType = AssemblyRegistry.ImportType(typeof(float)),
                            Value = 2.5
                        }
                    }
                })
            };

            ExpectedILFilePath = "TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreField_LoadIntLiteral()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, AssemblyRegistry.ImportType(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = AssemblyRegistry.ImportType(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new LiteralNode()
                            {
                                ReturnType = AssemblyRegistry.ImportType(typeof(int)),
                                Value = 1
                            }
                        }
                    }
                })
            };

            ExpectedILFilePath = "TestCanEmit_StoreField_LoadIntLiteral.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreLocalVariable_LoadField()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, AssemblyRegistry.ImportType(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("intLocal", AssemblyRegistry.ImportType(typeof(int)))
                        },
                        Initializer = new FieldNode()
                        {
                            Field = field
                        }
                    }
                })
            };

            ExpectedILFilePath = "TestCanEmit_StoreLocalVariable_LoadField.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral()
        {
            var property = new PropertyDefinition("doubleProperty", PropertyAttributes.HasDefault, AssemblyRegistry.ImportType(typeof(double)));
            var backingField = new FieldDefinition("doubleProperty_backingField", FieldAttributes.Static, AssemblyRegistry.ImportType(typeof(double)));

            typeEmitter.AddField(backingField);

            var setter = new MethodEmitter(typeEmitter, "set_doubleProperty", AssemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = setter.AddArgument(AssemblyRegistry.ImportType(typeof(double)), "value");
            setter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = AssemblyRegistry.ImportType(typeof(void)),
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

            var localVariable = new VariableDefinition("doubleLocal", AssemblyRegistry.ImportType(typeof(double)));

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
                            ReturnType = AssemblyRegistry.ImportType(typeof(double)),
                            Value = 5.5
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = AssemblyRegistry.ImportType(typeof(void)),
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

            ExpectedILFilePath = "TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral()
        {
            var property = new PropertyDefinition("stringProperty", PropertyAttributes.HasDefault, AssemblyRegistry.ImportType(typeof(string)));

            var getter = new MethodEmitter(typeEmitter, "get_stringProperty", AssemblyRegistry.ImportType(typeof(string)),
                MethodAttributes.Static | MethodAttributes.Private);

            getter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new LiteralNode()
                    {
                        ReturnType = AssemblyRegistry.ImportType(typeof(string)),
                        Value = "Test"
                    }
                })
            });

            property.GetMethod = getter.Get();
            typeEmitter.AddProperty(property);

            var methodWithArgument = new MethodEmitter(typeEmitter, "TestMethod", AssemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = methodWithArgument.AddArgument(AssemblyRegistry.ImportType(typeof(string)), "arg");

            methodWithArgument.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = AssemblyRegistry.ImportType(typeof(void)),
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
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Function = methodWithArgument.Get()
                        },
                        Arguments = new List<IExpressionNode>(new IExpressionNode[]
                        {
                            new LiteralNode()
                            {
                                ReturnType = AssemblyRegistry.ImportType(typeof(string)),
                                Value = "Test"
                            }
                        })
                    }
                })
            };

            ExpectedILFilePath = "TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral()
        {
            var callableMethod = new MethodEmitter(typeEmitter, "Test", AssemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Private | MethodAttributes.Static);

            callableMethod.AddArgument(AssemblyRegistry.ImportType(typeof(bool)), "isTrue");
            callableMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>(new IParserNode[]
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Function = callableMethod.Get()
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode
                            {
                                ReturnType = AssemblyRegistry.ImportType(typeof(bool)),
                                Value = true
                            }
                        }
                    }
                })
            };

            ExpectedILFilePath = "TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral.il";
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
                    AssemblyRegistry.ImportType(typeof(int)));
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

            ExpectedILFilePath = "TestCanEmit_MultipleNestedAssignments.il";
            Test();
        }

        #endregion

        #region Functor tests

        [TestMethod]
        public void TestCanEmit_FunctorDefinition()
        {
            FunctorTypeEmitter.Create(assemblyEmitter, AssemblyRegistry.ImportType(typeof(void)), new List<TypeReference>());

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_FunctorDefinition.il";
            Test();
        }

        [TestMethod]
        public void TestCanEmit_FunctorWithReturnTypeAndArguments()
        {
            FunctorTypeEmitter.Create(assemblyEmitter, AssemblyRegistry.ImportType(typeof(int)), 
                new List<TypeReference>(new TypeReference[]
                {
                    AssemblyRegistry.ImportType(typeof(bool)),
                    AssemblyRegistry.ImportType(typeof(float)),
                    AssemblyRegistry.ImportType(typeof(string)),
                }));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_FunctorWithReturnTypeAndArguments.il";
            Test();
        }

        #endregion

        #endregion

        #region Helpers

        public MethodBodyTests()
        {
            var tempLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            assemblyEmitter = new AssemblyEmitter(compilerArgs);
            typeEmitter = new TypeEmitter(assemblyEmitter, "klass");
        }

        private void Test()
        {
            var methodEmitter = new MethodEmitter(typeEmitter, "dummy", AssemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            methodEmitter.ParseTree(BodyCodeBlock);
            methodEmitter.SetAsEntryPoint();
            assemblyEmitter.Save();

            var il = Disassembler.DisassembleAssembly(assemblyEmitter.OutputPath);

            var expectedILPath = Path.Combine("..", "..", "ILTests", "MethodBodyTests", "MethodBodyTestsExpected", ExpectedILFilePath);
            var expectedIL = File.ReadAllText(expectedILPath, Encoding.UTF8);

            try
            {
                Assert.AreEqual(expectedIL.Trim(), il.Trim());
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
