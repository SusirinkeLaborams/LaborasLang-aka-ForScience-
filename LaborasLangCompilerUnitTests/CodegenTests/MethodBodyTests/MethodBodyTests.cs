using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.Codegen;
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
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Codegen.Methods;

namespace LaborasLangCompilerUnitTests.CodegenTests.MethodBodyTests
{
    [TestClass]
    public class MethodBodyTests : ILTestBase
    {
        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_EmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedOutput = string.Empty;
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_HelloWorld()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(string)), "Hello, world!"))
                }
            };

            ExpectedOutput = "Hello, world!";
            AssertSuccessByExecution();
        }

        #region Load/Store lvalues and load literals tests

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral()
        {
            var variable = new VariableDefinition("floatValue", assemblyEmitter.TypeToTypeReference(typeof(float)));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variable,
                        Initializer = new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(float)), 2.5)
                    },
                    CallConsoleWriteLine(new LocalVariableNode(variable))
                }
            };

            ExpectedOutput = 2.5.ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_StoreField_LoadIntLiteral()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(int)), 1)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = "1";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_StoreLocalVariable_LoadField()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(int)));
            typeEmitter.AddField(field);

            var variable = new VariableDefinition("intLocal", assemblyEmitter.TypeToTypeReference(typeof(int)));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(int)), 42)
                        }
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = variable,
                        Initializer = new FieldNode(field)
                    },
                    CallConsoleWriteLine(new LocalVariableNode(variable))
                }
            };

            ExpectedOutput = "42";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral()
        {
            var property = new PropertyDefinition("doubleProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeToTypeReference(typeof(double)));
            var backingField = new FieldDefinition("doubleProperty_backingField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(double)));

            typeEmitter.AddField(backingField);

            var setter = new MethodEmitter(typeEmitter, "set_doubleProperty", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = setter.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(double)), "value");
            setter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(backingField),
                            RightOperand = new FunctionArgumentNode()
                            {
                                Param = argument,
                                IsMethodStatic = true
                            }
                        }
                    }
                }
            });

            property.SetMethod = setter.Get().Resolve();

            typeEmitter.AddProperty(property);

            var localVariable = new VariableDefinition("doubleLocal", assemblyEmitter.TypeToTypeReference(typeof(double)));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable,
                        Initializer = new LiteralNode
                        {
                            ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(double)),
                            Value = 5.5
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new PropertyNode()
                            {
                                Property = property
                            },
                            RightOperand = new LocalVariableNode(localVariable)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(backingField))
                }
            };

            ExpectedOutput = 5.5.ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral()
        {
            var property = new PropertyDefinition("stringProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeToTypeReference(typeof(string)));

            var getter = new MethodEmitter(typeEmitter, "get_stringProperty", assemblyEmitter.TypeToTypeReference(typeof(string)),
                MethodAttributes.Static | MethodAttributes.Private);

            getter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(string)), "Test2")
                }
            });

            property.GetMethod = getter.Get().Resolve();
            typeEmitter.AddProperty(property);

            var methodWithArgument = new MethodEmitter(typeEmitter, "TestMethod", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = methodWithArgument.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(string)), "arg");

            methodWithArgument.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new FunctionArgumentNode()
                    {
                        Param = argument
                    }),
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
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
                    },
                    CallConsoleWriteLine(new FunctionArgumentNode()
                    {
                        Param = argument
                    })
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = methodWithArgument.Get()
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(string)), "Test1")
                        }
                    }
                }
            };

            ExpectedOutput = string.Format("{1}{0}{2}", Environment.NewLine, "Test1", "Test2");
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral()
        {
            var callableMethod = new MethodEmitter(typeEmitter, "Test", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Private | MethodAttributes.Static);

            callableMethod.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(bool)), "isTrue");
            callableMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new FunctionArgumentNode()
                    {
                        Param = callableMethod.Get().Parameters[0]
                    })
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = callableMethod.Get()
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode
                            {
                                ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(bool)),
                                Value = true
                            }
                        }
                    }
                }
            };

            ExpectedOutput = true.ToString();
            AssertSuccessByExecution();
        }

        #endregion

        #region Operator tests

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_MultipleNestedAssignments()
        {
            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode(assemblyEmitter.TypeToTypeReference(typeof(int)), 110)
            };

            const int count = 10;
            var fields = new List<FieldDefinition>();
            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Static | FieldAttributes.Private,
                    assemblyEmitter.TypeToTypeReference(typeof(int)));
                fields.Add(field);
                typeEmitter.AddField(field);

                assignmentNode.LeftOperand = new FieldNode(field);

                if (i != count - 1)
                {
                    var newNode = new AssignmentOperatorNode()
                    {
                        RightOperand = assignmentNode
                    };

                    assignmentNode = newNode;
                }
            }

            var outputCodeBlock = new CodeBlockNode()
            {
                Nodes = fields.Select(field => CallConsoleWriteLine(new FieldNode { Field = field })).ToList()
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = assignmentNode
                    },
                    outputCodeBlock
                }
            };

            ExpectedOutput = Enumerable.Repeat<int>(110, fields.Count).Select(i => i.ToString()).Aggregate((x, y) => x + Environment.NewLine + y);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_MultipleNestedInstanceFieldAssignments()
        {
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            typeEmitter.AddDefaultConstructor();
            var testMethod = new MethodEmitter(typeEmitter, "TestNestedInstanceFieldAssignment", voidType);

            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode(floatType, 110)
            };

            const int count = 10;
            var fields = new List<FieldDefinition>();

            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Private, floatType);
                typeEmitter.AddField(field);
                fields.Add(field);

                assignmentNode.LeftOperand = new FieldNode()
                {
                    ObjectInstance = new ThisNode
                    {
                        ExpressionReturnType = field.DeclaringType
                    },
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

            var outputCodeBlock = new CodeBlockNode()
            {
                Nodes = fields.Select(f => CallConsoleWriteLine(new FieldNode()
                {
                    Field = f, 
                    ObjectInstance = new ThisNode { ExpressionReturnType = f.DeclaringType }
                })).ToList()
            };

            testMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = assignmentNode
                    },
                    outputCodeBlock
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            ObjectInstance = new ObjectCreationNode()
                            {
                                ExpressionReturnType = typeEmitter.Get(assemblyEmitter),
                                Args = new IExpressionNode[] { }
                            },
                            Method = testMethod.Get(),
                        },
                        Args = new IExpressionNode[] { }
                    }
                }
            };

            ExpectedOutput = Enumerable.Repeat<int>(110, fields.Count).Select(i => i.ToString()).Aggregate((x, y) => x + Environment.NewLine + y);
            AssertSuccessByExecution();
        }

        #region Binary operators

        #region Arithmetic operators

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_AddIntegers()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var localVariable = new VariableDefinition("myVar", intType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new LocalVariableNode(localVariable),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = intType,
                                LeftOperand = new LiteralNode(intType, 2),
                                RightOperand = new LiteralNode(intType, 3)
                            }
                        }
                    },
                    CallConsoleWriteLine(new LocalVariableNode(localVariable))
                }
            };

            ExpectedOutput = "5";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_AddFloatAndInteger()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var localVariable = new VariableDefinition("myVar", doubleType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new LocalVariableNode(localVariable),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = floatType,
                                LeftOperand = new LiteralNode(floatType, 3.2f),
                                RightOperand = new LiteralNode(intType, 2)
                            }
                        }
                    },
                    CallConsoleWriteLine(new LocalVariableNode(localVariable))
                }
            };

            ExpectedOutput = ((double)(3.2f + 2)).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_AddStrings()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, stringType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = stringType,
                                LeftOperand = new LiteralNode(stringType, "testing string addition: "),
                                RightOperand = new LiteralNode(intType, 22)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = "testing string addition: 22";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_Subtraction()
        {
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, doubleType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = doubleType,
                                BinaryOperatorType = BinaryOperatorNodeType.Subtraction,
                                LeftOperand = new LiteralNode(doubleType, 3.4),
                                RightOperand = new LiteralNode(floatType, 5.5f)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (3.4 - 5.5f).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_Multiplication()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Multiplication,
                                LeftOperand = new LiteralNode(uintType, 5),
                                RightOperand = new LiteralNode(uintType, 3)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = "15";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_SignedDivision()
        {
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var longType = assemblyEmitter.TypeToTypeReference(typeof(long));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, longType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = doubleType,
                                BinaryOperatorType = BinaryOperatorNodeType.Division,
                                LeftOperand = new LiteralNode(doubleType, -15.4),
                                RightOperand = new LiteralNode(doubleType, 4.8)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = ((long)(-15.4 / 4.8)).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_UnsignedDivision()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Division,
                                LeftOperand = new LiteralNode(uintType, 17),
                                RightOperand = new LiteralNode(uintType, 3)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (17 / 3).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_SignedRemainder()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var longType = assemblyEmitter.TypeToTypeReference(typeof(long));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, longType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = intType,
                                BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                                LeftOperand = new LiteralNode(intType, 94),
                                RightOperand = new LiteralNode(intType, -17)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (94 % -17).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_UnsignedRemainder()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                                LeftOperand = new LiteralNode(uintType, 41),
                                RightOperand = new LiteralNode(uintType, 81)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (41 % 81).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_ShiftLeftAndRight()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.ShiftLeft,
                                LeftOperand = new BinaryOperatorNode()
                                {
                                    ExpressionReturnType = uintType,
                                    BinaryOperatorType = BinaryOperatorNodeType.ShiftRight,
                                    LeftOperand = new LiteralNode(uintType, 15),
                                    RightOperand = new LiteralNode(ushortType, 2)
                                },
                                RightOperand = new LiteralNode(uintType, 3)
                            }
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = ((15 >> 2) << 3).ToString();
            AssertSuccessByExecution();
        }

        #endregion

        #region Comparison operators

        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(TypeReference literalType, dynamic value1, dynamic value2)
        {
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var localA = new VariableDefinition("a", literalType);
            var localB = new VariableDefinition("b", literalType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localA,
                        Initializer = new LiteralNode(literalType, value1)
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = localB,
                        Initializer = new LiteralNode(literalType, value2)
                    },
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.GreaterThan,
                            LeftOperand = new LocalVariableNode(localA),
                            RightOperand = new LocalVariableNode(localB)
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "{0} is greater than {1}."), new LocalVariableNode(localA), new LocalVariableNode(localB))
                            }
                        },
                        FalseBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "{0} is not greater than {1}."), new LocalVariableNode(localA), new LocalVariableNode(localB))
                            }
                        }
                    },                    
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                            LeftOperand = new LocalVariableNode(localA),
                            RightOperand = new LocalVariableNode(localB)
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "{0} is less than {1}."), new LocalVariableNode(localA), new LocalVariableNode(localB))
                            }
                        },
                        FalseBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "{0} is not less than {1}."), new LocalVariableNode(localA), new LocalVariableNode(localB))
                            }
                        }
                    },
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.Equals,
                            LeftOperand = new LocalVariableNode(localA),
                            RightOperand = new LocalVariableNode(localB)
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "{0} and {1} are equal."), new LocalVariableNode(localA), new LocalVariableNode(localB))
                            }
                        },
                        FalseBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "{0} and {1} are not equal."), new LocalVariableNode(localA), new LocalVariableNode(localB))
                            }
                        }
                    }
                }
            };
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Numerals()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeToTypeReference(typeof(int)), 5, 6);

            ExpectedOutput =
                "5 is not greater than 6." + Environment.NewLine +
                "5 is less than 6." + Environment.NewLine +
                "5 and 6 are not equal.";

            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Strings()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeToTypeReference(typeof(string)), "hi", "bye");

            ExpectedOutput =
                "hi is greater than bye." + Environment.NewLine +
                "hi is not less than bye." + Environment.NewLine +
                "hi and bye are not equal.";

            AssertSuccessByExecution();
        }

        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(TypeReference literalType, dynamic value1, dynamic value2)
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var booleanType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var literal1 = new LiteralNode(literalType, value1);
            var literal2 = new LiteralNode(literalType, value2);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new LiteralNode(stringType, "Is {0} greater than or equal to {1}? {2}"), literal1, literal2, 
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = booleanType,
                            BinaryOperatorType = BinaryOperatorNodeType.GreaterEqualThan,
                            LeftOperand = literal1,
                            RightOperand = literal2
                        }),
                        
                    CallConsoleWriteLine(new LiteralNode(stringType, "Is {0} less than or equal to {1}? {2}"), literal1, literal2,
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = booleanType,
                            BinaryOperatorType = BinaryOperatorNodeType.LessEqualThan,
                            LeftOperand = literal1,
                            RightOperand = literal2
                        }),

                    CallConsoleWriteLine(new LiteralNode(stringType, "Is {0} not equal to {1}? {2}"), literal1, literal2,
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = booleanType,
                            BinaryOperatorType = BinaryOperatorNodeType.NotEquals,
                            LeftOperand = literal1,
                            RightOperand = literal2
                        })
                }
            };
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Numerals()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeToTypeReference(typeof(float)), 3.5, 2.1);

            ExpectedOutput = string.Format(
                "Is {0} greater than or equal to {1}? {2}{5}" +
                "Is {0} less than or equal to {1}? {3}{5}" +
                "Is {0} not equal to {1}? {4}",
                3.5f, 2.1f, 3.5f >= 2.1f, 3.5f <= 2.1f, 3.5f != 2.1f, Environment.NewLine);

            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Strings()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeToTypeReference(typeof(string)), "hi", "bye");

            ExpectedOutput = string.Format(
                "Is {0} greater than or equal to {1}? {2}{5}" +
                "Is {0} less than or equal to {1}? {3}{5}" +
                "Is {0} not equal to {1}? {4}",
                "hi", "bye", String.CompareOrdinal("hi", "bye") >= 0, String.CompareOrdinal("hi", "bye") <= 0, "hi" != "bye", Environment.NewLine);

            AssertSuccessByExecution();
        }

        #endregion

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_LogicalAnd_LogicalOr()
        {
            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<string>()
            {
                "System.String",
                "System.Boolean",                
                "System.Boolean",
                "System.Boolean"
            });

            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var booleanType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var variableDef1 = new VariableDefinition("a", booleanType);

            var variable1 = new LocalVariableNode()
            {
                LocalVariable = variableDef1
            };

            var variableDef2 = new VariableDefinition("b", booleanType);

            var variable2 = new LocalVariableNode()
            {
                LocalVariable = variableDef2
            };

            var literal1 = new LiteralNode()
            {
                ExpressionReturnType = booleanType,
                Value = true
            };

            var literal2 = new LiteralNode()
            {
                ExpressionReturnType = booleanType,
                Value = false
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef1,
                        Initializer = literal1
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef2,
                        Initializer = literal2
                    },

                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} && {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LogicalAnd,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} || {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LogicalOr,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_LogicalAnd_LogicalOr.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_BinaryAnd_BinaryOr_BinaryXor()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));

            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                uintType,
                uintType,
                uintType
            });

            var variableDef1 = new VariableDefinition("a", uintType);

            var variable1 = new LocalVariableNode()
            {
                LocalVariable = variableDef1
            };

            var variableDef2 = new VariableDefinition("b", uintType);

            var variable2 = new LocalVariableNode()
            {
                LocalVariable = variableDef2
            };

            var literal1 = new LiteralNode()
            {
                ExpressionReturnType = uintType,
                Value = 0x156
            };

            var literal2 = new LiteralNode()
            {
                ExpressionReturnType = uintType,
                Value = 0x841
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef1,
                        Initializer = literal1
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef2,
                        Initializer = literal2
                    },

                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} & {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryAnd,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} | {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryOr,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} ^ {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryXor,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_BinaryAnd_BinaryOr_BinaryXor.il";
            AssertSuccessByILComparison();
        }
        
        #endregion

        #region Unary operators

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_Negation_BinaryNot_Increment_Decrement()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                intType,
                intType,
                intType,
                intType,
                intType,
                intType,
                intType
            });

            var variableDefs = new List<VariableDefinition>();

            for(int i = 0; i < 7; i++)
            {
                variableDefs.Add(new VariableDefinition(((char)i + 'a').ToString(), intType));
            }

            var variables = variableDefs.Select(def => new LocalVariableNode()
                {
                    LocalVariable = def
                }).ToList();

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDefs[0],
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = intType,
                            Value = 5
                        }
                    }
                }
            };

            var nodes = (List<IParserNode>)BodyCodeBlock.Nodes;
            var operators = new UnaryOperatorNodeType[]
            {
                UnaryOperatorNodeType.BinaryNot,
                UnaryOperatorNodeType.Negation,
                UnaryOperatorNodeType.PostDecrement,
                UnaryOperatorNodeType.PostIncrement,
                UnaryOperatorNodeType.PreDecrement,
                UnaryOperatorNodeType.PreIncrement
            };

            for (int i = 1; i < 7; i++)
            {
                nodes.Add(new SymbolDeclarationNode()
                {
                    Variable = variableDefs[i],
                    Initializer = new UnaryOperatorNode()
                    {
                        UnaryOperatorType = operators[i - 1],
                        ExpressionReturnType = intType,
                        Operand = variables[0]
                    }
                });
            }

            nodes.Add(new MethodCallNode()
            {
                ExpressionReturnType = voidType,
                Function = new FunctionNode()
                {
                    Method = consoleWriteLine
                },
                Args = new List<IExpressionNode>()
                {
                    new LiteralNode()
                    {
                        ExpressionReturnType = stringType,
                        Value = "Results: \r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n"
                    },
                    variables[0],
                    variables[1],
                    variables[2],
                    variables[3],
                    variables[4],
                    variables[5],
                    variables[6]
                }
            });

            ExpectedILFilePath = "TestCanEmit_Negation_BinaryNot_Increment_Decrement.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_LogicalNot()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, boolType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>() 
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new UnaryOperatorNode()
                            {
                                ExpressionReturnType = boolType,
                                UnaryOperatorType = UnaryOperatorNodeType.LogicalNot,
                                Operand = new FieldNode(field)
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_LogicalNot.il";
            AssertSuccessByILComparison();
        }

        #endregion

        #endregion

        #region Control Flow Tests

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_WhileLoop()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                intType
            });

            var localVariableDef = new VariableDefinition("counter", intType);

            var localVariable = new LocalVariableNode()
            {
                LocalVariable = localVariableDef
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariableDef,
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = intType,
                            Value = 0
                        }
                    },
                    new WhileBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                            LeftOperand = localVariable,
                            RightOperand = new LiteralNode()
                            {
                                ExpressionReturnType = intType,
                                Value = 10
                            }
                        },
                        ExecutedBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ExpressionReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Method = consoleWriteLine
                                    },
                                    Args = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ExpressionReturnType = stringType,
                                            Value = "The loop has looped {0} time(s)."
                                        },
                                        localVariable
                                    }
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_WhileLoop.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_IfBlockWithoutElse()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType
            });

            var field = new FieldDefinition("myField", FieldAttributes.Static | FieldAttributes.Private, boolType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ConditionBlockNode()
                    {
                        Condition = new FieldNode(field),
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ExpressionReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Method = consoleWriteLine
                                    },
                                    Args = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ExpressionReturnType = stringType,
                                            Value = "myField is true."
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_IfBlockWithoutElse.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_Return()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var consoleReadLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "ReadLine", new List<TypeReference>());
            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                stringType
            });

            var readInputMethod = new MethodEmitter(typeEmitter, "ReadInput", stringType, MethodAttributes.Static | MethodAttributes.Private);

            readInputMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new MethodCallNode()
                        {
                            ExpressionReturnType = stringType,
                            Function = new FunctionNode()
                            {
                                Method = consoleReadLine
                            },
                            Args = new List<IExpressionNode>()
                        },
                    }
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = consoleWriteLine
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Your input is: \"{0}\"."
                            },
                            new MethodCallNode()
                            {
                                ExpressionReturnType = stringType,
                                Function = new FunctionNode()
                                {
                                    Method = readInputMethod.Get()
                                },
                                Args = new List<IExpressionNode>()
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Return.il";
            AssertSuccessByILComparison();
        }

        #endregion

        /* Missing tests:
         * Functors with optional parameters and params parameters
         */

        #region Method Call tests

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_CreateObject()
        {
            typeEmitter.AddDefaultConstructor();
            var myType = typeEmitter.Get(assemblyEmitter);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = new VariableDefinition("myInstance", myType),
                        Initializer = new ObjectCreationNode()
                        {
                            ExpressionReturnType = myType,
                            Args = new List<IExpressionNode>()
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CreateObject.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_CallFunctionWithOptionalParameter()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));

            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                stringType,
                stringType
            });

            var testMethod = new MethodEmitter(typeEmitter, "MethodWithDefaultParameter", voidType, MethodAttributes.Private | MethodAttributes.Static);
            
            var neededParameter = new ParameterDefinition("neededParameter", ParameterAttributes.None, stringType);
            testMethod.AddArgument(neededParameter);

            var optionalParameter = new ParameterDefinition("optionalParameter", ParameterAttributes.Optional | ParameterAttributes.HasDefault, stringType);
            optionalParameter.Constant = "Default value";
            testMethod.AddArgument(optionalParameter);

            testMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = consoleWriteLine
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0}: {1}"
                            },
                            new FunctionArgumentNode()
                            {
                                IsMethodStatic = true,
                                Param = neededParameter,
                            },
                            new FunctionArgumentNode()
                            {
                                IsMethodStatic = true,
                                Param = optionalParameter,
                            }
                        }
                    }
                }
            });

            var callableTestMethod1 = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter),
                "MethodWithDefaultParameter", new List<TypeReference>()
            {
                stringType
            });

            var callableTestMethod2 = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter),
                "MethodWithDefaultParameter", new List<TypeReference>()
            {
                stringType,
                stringType
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = callableTestMethod1
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Hi"
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = callableTestMethod2
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Hi"
                            },
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "NonOptional"
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CallFunctionWithOptionalParameter.il";
            AssertSuccessByILComparison();
        }

        #endregion
    }
}
