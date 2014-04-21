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
    public class MethodBodyTests : ILTestBase
    {
        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_EmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_EmptyMethod.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_HelloWorld()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Function = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", 
                                new List<string>() { "System.String" })
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = assemblyEmitter.TypeToTypeReference(typeof(string)),
                                Value = "Hello, world!"
                            }
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new MethodCallNode()
                        {
                            Function = new FunctionNode()
                            {
                                Function = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "ReadKey",
                                    new List<TypeReference>())
                            },
                            Arguments = new List<IExpressionNode>()
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_HelloWorld.il";
            Test();
        }

        #region Load/Store lvalues and load literals tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral()
        {
            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("floatValue", assemblyEmitter.TypeToTypeReference(typeof(float)))                            
                        },
                        Initializer = new LiteralNode()
                        {
                            ReturnType = assemblyEmitter.TypeToTypeReference(typeof(float)),
                            Value = 2.5
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new LiteralNode()
                            {
                                ReturnType = assemblyEmitter.TypeToTypeReference(typeof(int)),
                                Value = 1
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreField_LoadIntLiteral.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StoreLocalVariable_LoadField()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("intLocal", assemblyEmitter.TypeToTypeReference(typeof(int)))
                        },
                        Initializer = new FieldNode()
                        {
                            Field = field
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreLocalVariable_LoadField.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = backingField
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                Param = argument,
                                IsFunctionStatic = true
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
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable,
                        },
                        Initializer = new LiteralNode
                        {
                            ReturnType = assemblyEmitter.TypeToTypeReference(typeof(double)),
                            Value = 5.5
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
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
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral()
        {
            var property = new PropertyDefinition("stringProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeToTypeReference(typeof(string)));

            var getter = new MethodEmitter(typeEmitter, "get_stringProperty", assemblyEmitter.TypeToTypeReference(typeof(string)),
                MethodAttributes.Static | MethodAttributes.Private);

            getter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new LiteralNode()
                    {
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(string)),
                        Value = "Test"
                    }
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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
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
                            Function = methodWithArgument.Get()
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = assemblyEmitter.TypeToTypeReference(typeof(string)),
                                Value = "Test"
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral()
        {
            var callableMethod = new MethodEmitter(typeEmitter, "Test", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Private | MethodAttributes.Static);

            callableMethod.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(bool)), "isTrue");
            callableMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
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
                                ReturnType = assemblyEmitter.TypeToTypeReference(typeof(bool)),
                                Value = true
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral.il";
            Test();
        }

        #endregion

        #region Operator tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_MultipleNestedAssignments()
        {
            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode()
                {
                    ReturnType = assemblyEmitter.TypeToTypeReference(typeof(int)),
                    Value = 110
                }
            };

            const int count = 10;
            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Static | FieldAttributes.Private,
                    assemblyEmitter.TypeToTypeReference(typeof(int)));
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
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = assignmentNode
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_MultipleNestedAssignments.il";
            Test();
        }

        #region Binary operators

        #region Arithmetic operators

        [TestMethod, TestCategory("IL Tests")]
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
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new LocalVariableNode()
                            {
                                LocalVariable = localVariable
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = intType,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = intType,
                                    Value = 2
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = intType,
                                    Value = 3
                                },
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_AddIntegers.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new LocalVariableNode()
                            {
                                LocalVariable = localVariable
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = floatType,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = floatType,
                                    Value = 3.2f
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = intType,
                                    Value = 2
                                },
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_AddFloatAndInteger.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = stringType,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = stringType,
                                    Value = "testing string addition: "
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = intType,
                                    Value = 22
                                },
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_AddStrings.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = doubleType,
                                BinaryOperatorType = BinaryOperatorNodeType.Subtraction,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = doubleType,
                                    Value = 3.4
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = floatType,
                                    Value = 5.5f
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Subtraction.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Multiplication,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 5
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 3
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Multiplication.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = doubleType,
                                BinaryOperatorType = BinaryOperatorNodeType.Division,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = doubleType,
                                    Value = 15.4
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = doubleType,
                                    Value = 4.8
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_SignedDivision.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Division,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 17
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 3
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_UnsignedDivision.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = intType,
                                BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = intType,
                                    Value = 94
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = intType,
                                    Value = -17
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_SignedRemainder.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                                LeftOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 41
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 81
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_UnsignedRemainder.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.ShiftLeft,
                                LeftOperand = new BinaryOperatorNode()
                                {
                                    ReturnType = uintType,
                                    BinaryOperatorType = BinaryOperatorNodeType.ShiftRight,
                                    LeftOperand = new LiteralNode()
                                    {
                                        ReturnType = uintType,
                                        Value = 15
                                    },
                                    RightOperand = new LiteralNode()
                                    {
                                        ReturnType = ushortType,
                                        Value = 2
                                    }
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ReturnType = uintType,
                                    Value = 3
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_ShiftLeftAndRight.il";
            Test();
        }

        #endregion

        #region Comparison operators

        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(TypeReference literalType, dynamic value1, dynamic value2)
        {
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<string>()
            {
                "System.String",
                literalType.FullName,
                literalType.FullName
            });

            var localA = new VariableDefinition("a", literalType);
            var localB = new VariableDefinition("b", literalType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localA
                        },
                        Initializer = new LiteralNode()
                        {
                            ReturnType = literalType,
                            Value = value1
                        }
                    },
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localB
                        },
                        Initializer = new LiteralNode()
                        {
                            ReturnType = literalType,
                            Value = value2
                        }
                    },
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.GreaterThan,
                            LeftOperand = new LocalVariableNode()
                            {
                                LocalVariable = localA
                            },
                            RightOperand = new LocalVariableNode()
                            {
                                LocalVariable = localB
                            },
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Function = outputMethod,
                                    },
                                    Arguments = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ReturnType = stringType,
                                            Value = "{0} is greater than {1}."
                                        },
                                        new LocalVariableNode()
                                        {
                                            LocalVariable = localA
                                        },
                                        new LocalVariableNode()
                                        {
                                            LocalVariable = localB
                                        }
                                    }
                                }
                            }
                        },
                        FalseBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new ConditionBlockNode()
                                {
                                    Condition = new BinaryOperatorNode()
                                    {
                                        ReturnType = boolType,
                                        BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                                        LeftOperand = new LocalVariableNode()
                                        {
                                            LocalVariable = localA
                                        },
                                        RightOperand = new LocalVariableNode()
                                        {
                                            LocalVariable = localB
                                        },
                                    },
                                    TrueBlock = new CodeBlockNode()
                                    {
                                        Nodes = new List<IParserNode>()
                                        {
                                            new MethodCallNode()
                                            {
                                                ReturnType = voidType,
                                                Function = new FunctionNode()
                                                {
                                                    Function = outputMethod,
                                                },
                                                Arguments = new List<IExpressionNode>()
                                                {
                                                    new LiteralNode()
                                                    {
                                                        ReturnType = stringType,
                                                        Value = "{0} is less than {1}."
                                                    },
                                                    new LocalVariableNode()
                                                    {
                                                        LocalVariable = localA
                                                    },
                                                    new LocalVariableNode()
                                                    {
                                                        LocalVariable = localB
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    FalseBlock = new CodeBlockNode()
                                    {
                                        Nodes = new List<IParserNode>()
                                        {
                                            new ConditionBlockNode()
                                            {
                                                Condition = new BinaryOperatorNode()
                                                {
                                                    ReturnType = boolType,
                                                    BinaryOperatorType = BinaryOperatorNodeType.Equals,
                                                    LeftOperand = new LocalVariableNode()
                                                    {
                                                        LocalVariable = localA
                                                    },
                                                    RightOperand = new LocalVariableNode()
                                                    {
                                                        LocalVariable = localB
                                                    },
                                                },
                                                TrueBlock = new CodeBlockNode()
                                                {
                                                    Nodes = new List<IParserNode>()
                                                    {
                                                        new MethodCallNode()
                                                        {
                                                            ReturnType = voidType,
                                                            Function = new FunctionNode()
                                                            {
                                                                Function = outputMethod,
                                                            },
                                                            Arguments = new List<IExpressionNode>()
                                                            {
                                                                new LiteralNode()
                                                                {
                                                                    ReturnType = stringType,
                                                                    Value = "{0} and {1} are equal."
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localA
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localB
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                FalseBlock = new CodeBlockNode()
                                                {
                                                    Nodes = new List<IParserNode>()
                                                    {
                                                        new MethodCallNode()
                                                        {
                                                            ReturnType = voidType,
                                                            Function = new FunctionNode()
                                                            {
                                                                Function = outputMethod,
                                                            },
                                                            Arguments = new List<IExpressionNode>()
                                                            {
                                                                new LiteralNode()
                                                                {
                                                                    ReturnType = stringType,
                                                                    Value = "We're screwed."
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localA
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localB
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Numerals()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeToTypeReference(typeof(int)), 5, 6);

            ExpectedILFilePath = "TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Numerals.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Strings()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeToTypeReference(typeof(string)), "hi", "bye");

            ExpectedILFilePath = "TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Strings.il";
            Test();
        }

        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(TypeReference literalType, dynamic value1, dynamic value2)
        {
            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<string>()
            {
                "System.String",
                literalType.FullName,
                literalType.FullName,
                "System.Boolean"
            });

            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var booleanType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var literal1 = new LiteralNode()
            {
                ReturnType = literalType,
                Value = value1,
            };

            var literal2 = new LiteralNode()
            {
                ReturnType = literalType,
                Value = value2,
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "Is {0} is greater than or equal to {1}? {2}"
                            },
                            literal1,
                            literal2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.GreaterEqualThan,
                                LeftOperand = literal1,
                                RightOperand = literal2
                            }
                        }                        
                    },
                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "Is {0} is less than or equal to {1}? {2}"
                            },
                            literal1,
                            literal2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LessEqualThan,
                                LeftOperand = literal1,
                                RightOperand = literal2
                            }
                        }                        
                    },
                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "Is {0} is not equal to {1}? {2}"
                            },
                            literal1,
                            literal2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.NotEquals,
                                LeftOperand = literal1,
                                RightOperand = literal2
                            }
                        }                        
                    }
                }
            };
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Numerals()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeToTypeReference(typeof(float)), 3.5, 2.1);

            ExpectedILFilePath = "TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Numerals.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Strings()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeToTypeReference(typeof(string)), "hi", "bye");

            ExpectedILFilePath = "TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Strings.il";
            Test();
        }

        #endregion

        [TestMethod, TestCategory("IL Tests")]
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

            var variable1 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("a", booleanType)
            };

            var variable2 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("b", booleanType)
            };

            var literal1 = new LiteralNode()
            {
                ReturnType = booleanType,
                Value = true
            };

            var literal2 = new LiteralNode()
            {
                ReturnType = booleanType,
                Value = false
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable1,
                        Initializer = literal1
                    },
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable2,
                        Initializer = literal2
                    },

                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "{0} && {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LogicalAnd,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "{0} || {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LogicalOr,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_LogicalAnd_LogicalOr.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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


            var variable1 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("a", uintType)
            };

            var variable2 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("b", uintType)
            };

            var literal1 = new LiteralNode()
            {
                ReturnType = uintType,
                Value = 0x156
            };

            var literal2 = new LiteralNode()
            {
                ReturnType = uintType,
                Value = 0x841
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable1,
                        Initializer = literal1
                    },
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable2,
                        Initializer = literal2
                    },

                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "{0} & {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryAnd,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "{0} | {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryOr,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = outputMethod
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "{0} ^ {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryXor,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_BinaryAnd_BinaryOr_BinaryXor.il";
            Test();
        }
        
        #endregion

        #region Unary operators

        [TestMethod, TestCategory("IL Tests")]
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

            var variables = new List<LocalVariableNode>();

            for (int i = 0; i < 7; i++)
            {
                variables.Add(new LocalVariableNode()
                {
                    LocalVariable = new VariableDefinition(((char)i + 'a').ToString(), intType)
                });
            }

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variables[0],
                        Initializer = new LiteralNode()
                        {
                            ReturnType = intType,
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
                    DeclaredSymbol = variables[i],
                    Initializer = new UnaryOperatorNode()
                    {
                        UnaryOperatorType = operators[i - 1],
                        ReturnType = intType,
                        Operand = variables[0]
                    }
                });
            }

            nodes.Add(new MethodCallNode()
            {
                ReturnType = voidType,
                Function = new FunctionNode()
                {
                    Function = consoleWriteLine
                },
                Arguments = new List<IExpressionNode>()
                {
                    new LiteralNode()
                    {
                        ReturnType = stringType,
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
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new UnaryOperatorNode()
                            {
                                ReturnType = boolType,
                                UnaryOperatorType = UnaryOperatorNodeType.LogicalNot,
                                Operand = new FieldNode()
                                {
                                    Field = field
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_LogicalNot.il";
            Test();
        }

        #endregion

        #endregion

        #region Control Flow Tests

        [TestMethod, TestCategory("IL Tests")]
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

            var localVariable = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("counter", intType)
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = localVariable,
                        Initializer = new LiteralNode()
                        {
                            ReturnType = intType,
                            Value = 0
                        }
                    },
                    new WhileBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                            LeftOperand = localVariable,
                            RightOperand = new LiteralNode()
                            {
                                ReturnType = intType,
                                Value = 10
                            }
                        },
                        ExecutedBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Function = consoleWriteLine
                                    },
                                    Arguments = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ReturnType = stringType,
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
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                        Condition = new FieldNode()
                        {
                            Field = field,
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Function = consoleWriteLine
                                    },
                                    Arguments = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ReturnType = stringType,
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
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
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
                            ReturnType = stringType,
                            Function = new FunctionNode()
                            {
                                Function = consoleReadLine
                            },
                            Arguments = new List<IExpressionNode>()
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
                        ReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Function = consoleWriteLine
                        },
                        Arguments = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ReturnType = stringType,
                                Value = "Your input is: \"{0}\"."
                            },
                            new MethodCallNode()
                            {
                                ReturnType = stringType,
                                Function = new FunctionNode()
                                {
                                    Function = readInputMethod.Get()
                                },
                                Arguments = new List<IExpressionNode>()
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Return.il";
            Test();
        }

        #endregion

        /* Missing tests:
         * assign functor property to delegate
         * call with default parameters
         */

        #region Method Call tests

        [TestMethod, TestCategory("IL Tests")]
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
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("myInstance", myType)
                        },
                        Initializer = new ObjectCreationNode()
                        {
                            ReturnType = myType,
                            Arguments = new List<IExpressionNode>()
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CreateObject.il";
            Test();
        }

        #endregion


        #region Functor tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_FunctorDefinition()
        {
            FunctorTypeEmitter.Create(assemblyEmitter, assemblyEmitter.TypeToTypeReference(typeof(void)), new List<TypeReference>());

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_FunctorDefinition.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_FunctorWithReturnTypeAndArguments()
        {
            FunctorTypeEmitter.Create(assemblyEmitter, assemblyEmitter.TypeToTypeReference(typeof(int)),
                new List<TypeReference>()
                {
                    assemblyEmitter.TypeToTypeReference(typeof(bool)),
                    assemblyEmitter.TypeToTypeReference(typeof(float)),
                    assemblyEmitter.TypeToTypeReference(typeof(string)),
                });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_FunctorWithReturnTypeAndArguments.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_FunctionAssignmentToFunctorWithoutArgs()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType, new List<TypeReference>());
            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);

            var initializer = new FunctionNode()
            {
                Function = methodEmitter.Get(),
                ReturnType = functorType
            };

            typeEmitter.AddField(field, initializer);

            ExpectedILFilePath = "TestCanEmit_FunctionAssignmentToFunctorWithoutArgs.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_FunctionAssignmentToFunctorWithArgs()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType,
                new List<TypeReference>()
                {
                    assemblyEmitter.TypeToTypeReference(typeof(double)),
                    assemblyEmitter.TypeToTypeReference(typeof(float))
                });
            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);

            var initializer = new FunctionNode()
            {
                Function = methodEmitter.Get(),
                ReturnType = functorType
            };

            typeEmitter.AddField(field, initializer);

            ExpectedILFilePath = "TestCanEmit_FunctionAssignmentToFunctorWithArgs.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_FunctorAssignmentToDelegate()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var arguments = new List<TypeReference>()
                {
                    assemblyEmitter.TypeToTypeReference(typeof(int)),
                    assemblyEmitter.TypeToTypeReference(typeof(string))
                };

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, voidType, arguments);

            var functorField = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);
            typeEmitter.AddField(functorField);

            var declaringType = (TypeDefinition)typeEmitter.Get(assemblyEmitter);
            var delegateType = DelegateEmitter.Create(assemblyEmitter, declaringType, voidType, arguments);
            declaringType.NestedTypes.Add(delegateType);

            var delegateField = new FieldDefinition("myDelegate", FieldAttributes.Public | FieldAttributes.Static, delegateType);
            typeEmitter.AddField(delegateField);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = delegateField
                            },
                            RightOperand = new FieldNode()
                            {
                                Field = functorField
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_FunctorAssignmentToDelegate.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_FunctionAssignmentToDelegate()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var myMethod = methodEmitter.Get();
            var methodReturnType = myMethod.ReturnType;
            var methodArguments = myMethod.Parameters.Select(x => x.ParameterType).ToList();

            var declaringType = (TypeDefinition)typeEmitter.Get(assemblyEmitter);
            var delegateType = DelegateEmitter.Create(assemblyEmitter, declaringType, methodReturnType, methodArguments);
            declaringType.NestedTypes.Add(delegateType);

            var delegateField = new FieldDefinition("myDelegate", FieldAttributes.Public | FieldAttributes.Static, delegateType);
            typeEmitter.AddField(delegateField);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = delegateField
                            },
                            RightOperand = new FunctionNode()
                            {
                                ReturnType = delegateType,
                                Function = myMethod
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_FunctionAssignmentToDelegate.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_CallFunctor_PassReturnValueAsArgument()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType, new List<TypeReference>()
                {
                    stringType,
                    floatType
                });

            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);
            typeEmitter.AddField(field);

            var getFirstArgumentMethod = new MethodEmitter(typeEmitter, "GetString", stringType, MethodAttributes.Private | MethodAttributes.Static);

            getFirstArgumentMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new LiteralNode()
                    {
                        ReturnType = stringType,
                        Value = "Str"
                    }
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new MethodCallNode()
                        {
                            Function = new FieldNode()
                            {
                                Field = field
                            },
                            Arguments = new List<IExpressionNode>()
                            {
                                new MethodCallNode()
                                {
                                    Arguments = new List<IExpressionNode>(),
                                    Function = new FunctionNode()
                                    {
                                        ReturnType = stringType,
                                        Function = getFirstArgumentMethod.Get()
                                    },
                                    ReturnType = stringType
                                },
                                new LiteralNode()
                                {
                                    ReturnType = floatType,
                                    Value = 3.5f
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CallFunctor_PassReturnValueAsArgument.il";
            Test();
        }

        #endregion
    }
}
