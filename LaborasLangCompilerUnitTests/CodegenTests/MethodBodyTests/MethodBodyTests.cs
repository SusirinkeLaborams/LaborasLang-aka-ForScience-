using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaborasLangCompilerUnitTests.CodegenTests.MethodBodyTests
{
    [TestClass]
    public class MethodBodyTests : CodegenTestBase
    {
        public MethodBodyTests()
        {
        }

        internal MethodBodyTests(AssemblyEmitter assemblyEmitter, string className, bool bulkTesting) :
            base(assemblyEmitter, className, bulkTesting)
        {
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_EmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedOutput = string.Empty;
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_HelloWorld()
        {
            GenerateBodyToOutputExpression(new LiteralNode(assemblyEmitter.TypeSystem.String, "Hello, world!"));
            ExpectedOutput = "Hello, world!";
            AssertSuccessByExecution();
        }

        #region Load/Store lvalues and load literals tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral()
        {
            var variable = new VariableDefinition("floatValue", assemblyEmitter.TypeSystem.Single);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variable,
                        Initializer = new LiteralNode(assemblyEmitter.TypeSystem.Single, 2.5)
                    },
                    CallConsoleWriteLine(new LocalVariableNode(variable))
                }
            };

            ExpectedOutput = 2.5.ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StoreField_LoadIntLiteral()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeSystem.Int32);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 1)
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = "1";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StoreLocalVariable_LoadField()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeSystem.Int32);
            typeEmitter.AddField(field);

            var variable = new VariableDefinition("intLocal", assemblyEmitter.TypeSystem.Int32);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 42)
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

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral()
        {
            var property = new PropertyDefinition("doubleProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeSystem.Double);
            var backingField = new FieldDefinition("doubleProperty_backingField", FieldAttributes.Static, assemblyEmitter.TypeSystem.Double);

            typeEmitter.AddField(backingField);

            var setter = new MethodEmitter(typeEmitter, "set_doubleProperty", assemblyEmitter.TypeSystem.Void,
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = setter.AddArgument(assemblyEmitter.TypeSystem.Double, "value");
            setter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(backingField),
                        RightOperand = new ParameterNode(argument)
                    }
                }
            });

            property.SetMethod = setter.Get().Resolve();

            typeEmitter.AddProperty(property);

            var localVariable = new VariableDefinition("doubleLocal", assemblyEmitter.TypeSystem.Double);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable,
                        Initializer = new LiteralNode(assemblyEmitter.TypeSystem.Double, 5.5)
                    },
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new PropertyNode(assemblyEmitter, property),
                        RightOperand = new LocalVariableNode(localVariable)
                    },
                    CallConsoleWriteLine(new FieldNode(backingField))
                }
            };

            ExpectedOutput = 5.5.ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral()
        {
            var property = new PropertyDefinition("stringProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeSystem.String);

            var getter = new MethodEmitter(typeEmitter, "get_stringProperty", assemblyEmitter.TypeSystem.String,
                MethodAttributes.Static | MethodAttributes.Private);

            getter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode() 
                    {
                        Expression = new LiteralNode(assemblyEmitter.TypeSystem.String, "Test2")
                    }
                }
            });

            property.GetMethod = getter.Get().Resolve();
            typeEmitter.AddProperty(property);

            var methodWithArgument = new MethodEmitter(typeEmitter, "TestMethod", assemblyEmitter.TypeSystem.Void,
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = methodWithArgument.AddArgument(assemblyEmitter.TypeSystem.String, "arg");

            methodWithArgument.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new ParameterNode(argument)),
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new ParameterNode(argument),
                        RightOperand = new PropertyNode(assemblyEmitter, property)
                    },
                    CallConsoleWriteLine(new ParameterNode(argument))
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = methodWithArgument.Get().GetReturnType(),
                        Function = new FunctionNode()
                        {
                            Method = methodWithArgument.Get()
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode(assemblyEmitter.TypeSystem.String, "Test1")
                        }
                    }
                }
            };

            ExpectedOutput = string.Format("{1}{0}{2}", Environment.NewLine, "Test1", "Test2");
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral()
        {
            var callableMethod = new MethodEmitter(typeEmitter, "Test", assemblyEmitter.TypeSystem.Void,
                MethodAttributes.Private | MethodAttributes.Static);

            callableMethod.AddArgument(assemblyEmitter.TypeSystem.Boolean, "isTrue");
            callableMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new ParameterNode(callableMethod.Get().Parameters[0]))
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
                            new LiteralNode(assemblyEmitter.TypeSystem.Boolean, true)
                        }
                    }
                }
            };

            ExpectedOutput = true.ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_Null()
        {
            var localVariable = new VariableDefinition(assemblyEmitter.TypeSystem.Object);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable
                    },
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = assemblyEmitter.TypeSystem.Boolean,
                            BinaryOperatorType = BinaryOperatorNodeType.NotEquals,
                            LeftOperand = new LocalVariableNode(localVariable),
                            RightOperand = new NullNode()
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeSystem.String, "Empty variable is not null."))
                            }
                        },
                        FalseBlock = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeSystem.String, "Empty variable is null."))
                            }
                        }
                    }
                }
            };

            ExpectedOutput = "Empty variable is null.";
            AssertSuccessByExecution();
        }

        #endregion

        #region Operator tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_MultipleNestedAssignments()
        {
            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 110)
            };

            const int count = 10;
            var fields = new List<FieldDefinition>();
            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Static | FieldAttributes.Private,
                    assemblyEmitter.TypeSystem.Int32);
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
                Nodes = fields.Select(field => CallConsoleWriteLine(new FieldNode(field))).ToList()
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    assignmentNode,
                    outputCodeBlock
                }
            };

            ExpectedOutput = Enumerable.Repeat<int>(110, fields.Count).Select(i => i.ToString()).Aggregate((x, y) => x + Environment.NewLine + y);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_MultipleNestedInstanceFieldAssignments()
        {
            var floatType = assemblyEmitter.TypeSystem.Single;
            var voidType = assemblyEmitter.TypeSystem.Void;

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
                    assignmentNode,
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
                            ObjectInstance = ConstructTypeEmitterInstance(),
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

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_AddIntegers()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var localVariable = new VariableDefinition("myVar", intType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable
                    },
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new LocalVariableNode(localVariable),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = intType,
                            LeftOperand = new LiteralNode(intType, 2),
                            RightOperand = new LiteralNode(intType, 3)
                        }
                    },
                    CallConsoleWriteLine(new LocalVariableNode(localVariable))
                }
            };

            ExpectedOutput = "5";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_AddFloatAndInteger()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var floatType = assemblyEmitter.TypeSystem.Single;
            var doubleType = assemblyEmitter.TypeSystem.Double;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var localVariable = new VariableDefinition("myVar", doubleType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable
                    },
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new LocalVariableNode(localVariable),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = floatType,
                            LeftOperand = new LiteralNode(floatType, 3.2f),
                            RightOperand = new LiteralNode(intType, 2)
                        }
                    },
                    CallConsoleWriteLine(new LocalVariableNode(localVariable))
                }
            };

            ExpectedOutput = ((double)(3.2f + 2)).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_AddStrings()
        {
            var stringType = assemblyEmitter.TypeSystem.String;
            var intType = assemblyEmitter.TypeSystem.Int32;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, stringType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = stringType,
                            LeftOperand = new LiteralNode(stringType, "testing string addition: "),
                            RightOperand = new LiteralNode(intType, 22)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = "testing string addition: 22";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_Subtraction()
        {
            var doubleType = assemblyEmitter.TypeSystem.Double;
            var floatType = assemblyEmitter.TypeSystem.Single;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, doubleType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = doubleType,
                            BinaryOperatorType = BinaryOperatorNodeType.Subtraction,
                            LeftOperand = new LiteralNode(doubleType, 3.4),
                            RightOperand = new LiteralNode(floatType, 5.5f)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (3.4 - 5.5f).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_Multiplication()
        {
            var uintType = assemblyEmitter.TypeSystem.UInt32;
            var ushortType = assemblyEmitter.TypeSystem.UInt16;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = uintType,
                            BinaryOperatorType = BinaryOperatorNodeType.Multiplication,
                            LeftOperand = new LiteralNode(uintType, 5),
                            RightOperand = new LiteralNode(uintType, 3)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = "15";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_SignedDivision()
        {
            var doubleType = assemblyEmitter.TypeSystem.Double;
            var longType = assemblyEmitter.TypeSystem.Int64;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, longType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = doubleType,
                            BinaryOperatorType = BinaryOperatorNodeType.Division,
                            LeftOperand = new LiteralNode(doubleType, -15.4),
                            RightOperand = new LiteralNode(doubleType, 4.8)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = ((long)(-15.4 / 4.8)).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_UnsignedDivision()
        {
            var uintType = assemblyEmitter.TypeSystem.UInt32;
            var ushortType = assemblyEmitter.TypeSystem.UInt16;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = uintType,
                            BinaryOperatorType = BinaryOperatorNodeType.Division,
                            LeftOperand = new LiteralNode(uintType, 17),
                            RightOperand = new LiteralNode(uintType, 3)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (17 / 3).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_SignedRemainder()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var longType = assemblyEmitter.TypeSystem.Int64;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, longType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = intType,
                            BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                            LeftOperand = new LiteralNode(intType, 94),
                            RightOperand = new LiteralNode(intType, -17)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (94 % -17).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_UnsignedRemainder()
        {
            var uintType = assemblyEmitter.TypeSystem.UInt32;
            var ushortType = assemblyEmitter.TypeSystem.UInt16;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = uintType,
                            BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                            LeftOperand = new LiteralNode(uintType, 41),
                            RightOperand = new LiteralNode(uintType, 81)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = (41 % 81).ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ShiftLeftAndRight()
        {
            var uintType = assemblyEmitter.TypeSystem.UInt32;
            var ushortType = assemblyEmitter.TypeSystem.UInt16;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
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
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = ((15 >> 2) << 3).ToString();
            AssertSuccessByExecution();
        }

        #endregion

        #region Comparison operators

        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(TypeReference literalType, IConvertible value1, IConvertible value2)
        {
            var boolType = assemblyEmitter.TypeSystem.Boolean;
            var stringType = assemblyEmitter.TypeSystem.String;
            var voidType = assemblyEmitter.TypeSystem.Void;

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

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Numerals()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeSystem.Int32, 5, 6);

            ExpectedOutput =
                "5 is not greater than 6." + Environment.NewLine +
                "5 is less than 6." + Environment.NewLine +
                "5 and 6 are not equal.";

            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Strings()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeSystem.String, "hi", "bye");

            ExpectedOutput =
                "hi is greater than bye." + Environment.NewLine +
                "hi is not less than bye." + Environment.NewLine +
                "hi and bye are not equal.";

            AssertSuccessByExecution();
        }

        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(TypeReference literalType, IConvertible value1, IConvertible value2)
        {
            var voidType = assemblyEmitter.TypeSystem.Void;
            var stringType = assemblyEmitter.TypeSystem.String;
            var booleanType = assemblyEmitter.TypeSystem.Boolean;

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

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Numerals()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeSystem.Single, 3.5, 2.1);

            ExpectedOutput = string.Format(
                "Is {0} greater than or equal to {1}? {2}{5}" +
                "Is {0} less than or equal to {1}? {3}{5}" +
                "Is {0} not equal to {1}? {4}",
                3.5f, 2.1f, 3.5f >= 2.1f, 3.5f <= 2.1f, 3.5f != 2.1f, Environment.NewLine);

            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Strings()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeSystem.String, "hi", "bye");

            ExpectedOutput = string.Format(
                "Is {0} greater than or equal to {1}? {2}{5}" +
                "Is {0} less than or equal to {1}? {3}{5}" +
                "Is {0} not equal to {1}? {4}",
                "hi", "bye", String.CompareOrdinal("hi", "bye") >= 0, String.CompareOrdinal("hi", "bye") <= 0, "hi" != "bye", Environment.NewLine);

            AssertSuccessByExecution();
        }

        #endregion

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_LogicalAnd_LogicalOr()
        {
            var stringType = assemblyEmitter.TypeSystem.String;
            var booleanType = assemblyEmitter.TypeSystem.Boolean;

            var variableDef1 = new VariableDefinition("a", booleanType);
            var variable1 = new LocalVariableNode(variableDef1);

            var variableDef2 = new VariableDefinition("b", booleanType);
            var variable2 = new LocalVariableNode(variableDef2);

            const bool kValue1 = true;
            const bool kValue2 = false;

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef1,
                        Initializer = new LiteralNode(booleanType, kValue1)
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef2,
                        Initializer = new LiteralNode(booleanType, kValue2)
                    },
                    
                    CallConsoleWriteLine(new LiteralNode(stringType, "{0} && {1} == {2}."), variable1, variable2,
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = booleanType,
                            BinaryOperatorType = BinaryOperatorNodeType.LogicalAnd,
                            LeftOperand = variable1,
                            RightOperand = variable2
                        }),

                    CallConsoleWriteLine(new LiteralNode(stringType, "{0} || {1} == {2}."), variable1, variable2,
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = booleanType,
                            BinaryOperatorType = BinaryOperatorNodeType.LogicalOr,
                            LeftOperand = variable1,
                            RightOperand = variable2
                        })
                }
            };

            ExpectedOutput = string.Format("{0} && {1} == {2}.{4}{0} || {1} == {3}.", kValue1, kValue2, kValue1 && kValue2, kValue1 || kValue2, Environment.NewLine);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_BinaryAnd_BinaryOr_BinaryXor()
        {
            var stringType = assemblyEmitter.TypeSystem.String;
            var uintType = assemblyEmitter.TypeSystem.UInt32;

            var variableDef1 = new VariableDefinition("a", uintType);
            var variable1 = new LocalVariableNode(variableDef1);

            var variableDef2 = new VariableDefinition("b", uintType);
            var variable2 = new LocalVariableNode(variableDef2);

            const int kValue1 = 0x156;
            const int kValue2 = 0x841;

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef1,
                        Initializer = new LiteralNode(uintType, kValue1)
                    },
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDef2,
                        Initializer = new LiteralNode(uintType, kValue2)
                    },
                    
                    CallConsoleWriteLine(new LiteralNode(stringType, "{0} & {1} == {2}."), variable1, variable2, 
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = uintType,
                            BinaryOperatorType = BinaryOperatorNodeType.BinaryAnd,
                            LeftOperand = variable1,
                            RightOperand = variable2
                        }),

                    CallConsoleWriteLine(new LiteralNode(stringType, "{0} | {1} == {2}."), variable1, variable2, 
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = uintType,
                            BinaryOperatorType = BinaryOperatorNodeType.BinaryOr,
                            LeftOperand = variable1,
                            RightOperand = variable2
                        }),

                    CallConsoleWriteLine(new LiteralNode(stringType, "{0} ^ {1} == {2}."), variable1, variable2, 
                        new BinaryOperatorNode()
                        {
                            ExpressionReturnType = uintType,
                            BinaryOperatorType = BinaryOperatorNodeType.BinaryXor,
                            LeftOperand = variable1,
                            RightOperand = variable2
                        })
                }
            };

            ExpectedOutput = string.Format("{0} & {1} == {2}.{5}{0} | {1} == {3}.{5}{0} ^ {1} == {4}.",
                kValue1, kValue2, kValue1 & kValue2, kValue1 | kValue2, kValue1 ^ kValue2, Environment.NewLine);

            AssertSuccessByExecution();
        }

        #endregion

        #region Unary operators

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_Negation_BinaryNot()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;

            var variableDefs = new List<VariableDefinition>();
            var operators = new UnaryOperatorNodeType[]
            {
                UnaryOperatorNodeType.BinaryNot,
                UnaryOperatorNodeType.Negation,
            };

            for (int i = 0; i < operators.Length + 1; i++)
            {
                variableDefs.Add(new VariableDefinition(((char)(i + 'a')).ToString(), intType));
            }

            var variables = variableDefs.Select(def => new LocalVariableNode(def)).ToList();

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDefs[0],
                        Initializer = new LiteralNode(intType, 5)
                    }
                }
            };

            var nodes = (List<IParserNode>)BodyCodeBlock.Nodes;
            for (int i = 1; i < operators.Length + 1; i++)
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

            nodes.Add(CallConsoleWriteLine(
                new LiteralNode(stringType, "Results: \r\n{0}\r\n{1}\r\n{2}"),
                variables[0],
                variables[1],
                variables[2]
            ));

            ExpectedOutput = "Results: \r\n5\r\n-6\r\n-5\r\n";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IncrementDecrement()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;

            var variableDefs = new List<VariableDefinition>();

            var operators = new IncrementDecrementOperatorType[]
            {
                IncrementDecrementOperatorType.PostDecrement,
                IncrementDecrementOperatorType.PostIncrement,
                IncrementDecrementOperatorType.PreDecrement,
                IncrementDecrementOperatorType.PreIncrement
            };

            for (int i = 0; i < operators.Length + 1; i++)
            {
                variableDefs.Add(new VariableDefinition(((char)i + 'a').ToString(), intType));
            }

            var variables = variableDefs.Select(def => new LocalVariableNode(def)).ToList();

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variableDefs[0],
                        Initializer = new LiteralNode(intType, 5)
                    }
                }
            };

            var nodes = (List<IParserNode>)BodyCodeBlock.Nodes;

            for (int i = 1; i < operators.Length + 1; i++)
            {
                nodes.Add(new SymbolDeclarationNode()
                {
                    Variable = variableDefs[i],
                    Initializer = new IncrementDecrementOperatorNode()
                    {
                        IncrementDecrementType = operators[i - 1],
                        Operand = variables[0]
                    }
                });
            }

            nodes.Add(CallConsoleWriteLine(
                new LiteralNode(stringType, "Results: \r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}"),
                variables[0],
                variables[1],
                variables[2],
                variables[3],
                variables[4]
            ));

            ExpectedOutput = "Results: \r\n5\r\n5\r\n4\r\n4\r\n5\r\n";
            AssertSuccessByExecution();

        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IncrementField()
        {
            var field = new FieldDefinition("fieldToIncrement", FieldAttributes.Private, assemblyEmitter.TypeSystem.Int32);
            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, new LiteralNode(assemblyEmitter.TypeSystem.Int32, 3));

            LocalVariableNode objectInstance;

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    ConstructTypeEmitterInstance(out objectInstance),
                    CallConsoleWriteLine(new FieldNode()
                    {
                        ObjectInstance = objectInstance,
                        Field = field
                    }),
                    new IncrementDecrementOperatorNode()
                    {
                        IncrementDecrementType = IncrementDecrementOperatorType.PostIncrement,
                        Operand = new FieldNode()
                        {
                            ObjectInstance = objectInstance,
                            Field = field
                        }
                    },
                    CallConsoleWriteLine(new FieldNode()
                    {
                        ObjectInstance = objectInstance,
                        Field = field
                    })
                }
            };

            ExpectedOutput = "3" + Environment.NewLine + "4";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_DecrementArrayItem()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1);
            var arrayVariable = new VariableDefinition(arrayType);
            var arrayNode = new LocalVariableNode(arrayVariable);

            var arrayAccessNode = new ArrayAccessNode()
            {
                ExpressionReturnType = assemblyEmitter.TypeSystem.Int32,
                ObjectInstance = arrayNode,
                Indices = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 0) }
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = arrayVariable,
                        Initializer = new ArrayCreationNode()
                        {
                            ExpressionReturnType = arrayType,
                            Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 1) },
                            Initializer = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 9) }
                        }
                    },
                    CallConsoleWriteLine(arrayAccessNode),
                    new IncrementDecrementOperatorNode()
                    {
                        IncrementDecrementType = IncrementDecrementOperatorType.PostDecrement,
                        Operand = arrayAccessNode
                    },
                    CallConsoleWriteLine(arrayAccessNode)
                }
            };

            ExpectedOutput = "9" + Environment.NewLine + "8";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_LogicalNot()
        {
            var voidType = assemblyEmitter.TypeSystem.Void;
            var boolType = assemblyEmitter.TypeSystem.Boolean;

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, boolType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>() 
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new UnaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            UnaryOperatorType = UnaryOperatorNodeType.LogicalNot,
                            Operand = new FieldNode(field)
                        }
                    },
                    CallConsoleWriteLine(new FieldNode(field))
                }
            };

            ExpectedOutput = true.ToString();
            AssertSuccessByExecution();
        }

        #endregion

        #endregion

        #region Control Flow Tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_WhileLoop()
        {
            var stringType = assemblyEmitter.TypeSystem.String;
            var intType = assemblyEmitter.TypeSystem.Int32;
            var boolType = assemblyEmitter.TypeSystem.Boolean;

            var localVariableDef = new VariableDefinition("counter", intType);
            var localVariable = new LocalVariableNode(localVariableDef);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariableDef,
                        Initializer = new LiteralNode(intType, 0)
                    },
                    new WhileBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                            LeftOperand = localVariable,
                            RightOperand = new LiteralNode(intType, 10)
                        },
                        ExecutedBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "The loop has looped {0} time(s)."),
                                    new IncrementDecrementOperatorNode()
                                    {
                                        IncrementDecrementType = IncrementDecrementOperatorType.PostIncrement,
                                        Operand = localVariable
                                    })
                            }
                        }
                    }
                }
            };

            ExpectedOutput = Enumerable.Range(0, 10).Select(i => string.Format("The loop has looped {0} time(s).", i) + Environment.NewLine).Aggregate((x, y) => x + y);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IfBlockWithoutElse()
        {
            var stringType = assemblyEmitter.TypeSystem.String;
            var voidType = assemblyEmitter.TypeSystem.Void;
            var boolType = assemblyEmitter.TypeSystem.Boolean;

            var field = new FieldDefinition("myField", FieldAttributes.Static | FieldAttributes.Private, boolType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(field),
                        RightOperand = new LiteralNode(boolType, true)
                    },
                    new ConditionBlockNode()
                    {
                        Condition = new FieldNode(field),
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                CallConsoleWriteLine(new LiteralNode(stringType, "myField is true."))
                            }
                        }
                    }
                }
            };

            ExpectedOutput = "myField is true.";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_Return()
        {
            var stringType = assemblyEmitter.TypeSystem.String;

            var readInputMethod = new MethodEmitter(typeEmitter, "ReadInput", stringType, MethodAttributes.Static | MethodAttributes.Private);

            readInputMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new LiteralNode(stringType, "<some random input>")
                    }
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new LiteralNode(stringType, "Your input is: \"{0}\"."),
                        new MethodCallNode()
                        {
                            ExpressionReturnType = stringType,
                            Function = new FunctionNode()
                            {
                                Method = readInputMethod.Get()
                            },
                            Args = new List<IExpressionNode>()
                        })
                }
            };

            ExpectedOutput = "Your input is: \"<some random input>\".";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestForLoop()
        {
            var indexVariable = new VariableDefinition("i", assemblyEmitter.TypeSystem.Int32);
            var indexVariableNode = new LocalVariableNode(indexVariable);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ForLoopNode()
                    {
                        InitializationBlock = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                new SymbolDeclarationNode()
                                {
                                    Variable = indexVariable,
                                    Initializer = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 0)
                                }
                            }
                        },
                        ConditionBlock = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = assemblyEmitter.TypeSystem.Boolean,
                            BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                            LeftOperand = indexVariableNode,
                            RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 10)
                        },
                        IncrementBlock = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                new IncrementDecrementOperatorNode()
                                {
                                    IncrementDecrementType = IncrementDecrementOperatorType.PostIncrement,
                                    Operand = indexVariableNode
                                }
                            }
                        },
                        Body = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                CallConsoleWriteLine(indexVariableNode)
                            }
                        }
                    }
                }
            };

            ExpectedOutput = Enumerable.Range(0, 10).Select(i => i + Environment.NewLine).Aggregate((x, y) => x + y);
            AssertSuccessByExecution();
        }

        private void TestForEachLoopHelper(IExpressionNode collection, TypeReference elementType, string expectedOutput)
        {
            var loopVariable = new VariableDefinition(elementType);
            var loopVariableNode = new LocalVariableNode(loopVariable);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ForEachLoop()
                    {
                        Collection = collection,
                        LoopVariable = new SymbolDeclarationNode()
                        {
                            Variable = loopVariable
                        },
                        Body = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                CallConsoleWriteLine(loopVariableNode)
                            }
                        }
                    }
                }
            };

            ExpectedOutput = expectedOutput;
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestForEachLoop_Vector()
        {
            var arrayCreation = new ArrayCreationNode()
            {
                ExpressionReturnType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1),
                Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 10) },
                Initializer = Enumerable.Range(0, 10).Select(i => new LiteralNode(assemblyEmitter.TypeSystem.Int32, i)).ToArray()
            };

            var expectedOutput = Enumerable.Range(0, 10).Select(i => i + Environment.NewLine).Aggregate((x, y) => x + y);
            TestForEachLoopHelper(arrayCreation, assemblyEmitter.TypeSystem.Int32, expectedOutput);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestForEachLoop_IntArray()
        {
            var arrayCreation = new ArrayCreationNode()
            {
                ExpressionReturnType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 3),
                Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2), new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2), new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2) },
                Initializer = Enumerable.Range(0, 8).Select(i => new LiteralNode(assemblyEmitter.TypeSystem.Int32, i)).ToArray()
            };

            var expectedOutput = Enumerable.Range(0, 8).Select(i => i + Environment.NewLine).Aggregate((x, y) => x + y);
            TestForEachLoopHelper(arrayCreation, assemblyEmitter.TypeSystem.Int32, expectedOutput);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestForEachLoop_StringArray()
        {
            var values = new[]
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight"
            };

            var arrayCreation = new ArrayCreationNode()
            {
                ExpressionReturnType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 3),
                Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2), new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2), new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2) },
                Initializer = values.Select(v => new LiteralNode(assemblyEmitter.TypeSystem.String, v)).ToArray()
            };

            var expectedOutput = values.Select(i => i + Environment.NewLine).Aggregate((x, y) => x + y);
            TestForEachLoopHelper(arrayCreation, assemblyEmitter.TypeSystem.String, expectedOutput);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestForEachLoop_List()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1);
            var arrayCreation = new ArrayCreationNode()
            {
                ExpressionReturnType = arrayType,
                Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 10) },
                Initializer = Enumerable.Range(0, 10).Select(i => new LiteralNode(assemblyEmitter.TypeSystem.Int32, i)).ToArray()
            };

            var listType = AssemblyRegistry.FindType(assemblyEmitter, "System.Collections.Generic.List`1").MakeGenericType(assemblyEmitter.TypeSystem.Int32);
            var listCreation = new ObjectCreationNode()
            {
                ExpressionReturnType = listType,
                Constructor = AssemblyRegistry.GetCompatibleConstructor(assemblyEmitter, listType, new TypeReference[] { listType }),
                Args = new IExpressionNode[] { arrayCreation }
            };

            var expectedOutput = Enumerable.Range(0, 10).Select(i => i + Environment.NewLine).Aggregate((x, y) => x + y);
            TestForEachLoopHelper(listCreation, assemblyEmitter.TypeSystem.Int32, expectedOutput);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestForEachLoop_CustomCollection()
        {
            var systemValueType = AssemblyRegistry.FindType(assemblyEmitter, "System.ValueType");

            var collectionType = new TypeEmitter(assemblyEmitter, "Collection", "", TypeEmitter.DefaultTypeAttributes, systemValueType);
            var enumeratorType = new TypeEmitter(assemblyEmitter, "Enumerator", "", TypeEmitter.DefaultTypeAttributes, systemValueType);
            var collectionTypeRef = collectionType.Get(assemblyEmitter);
            var enumeratorTypeRef = enumeratorType.Get(assemblyEmitter);

            var counterField = new FieldDefinition("counter", FieldAttributes.Private, assemblyEmitter.TypeSystem.Int32);
            var counterFieldNode = new FieldNode()
            {
                ObjectInstance = new ThisNode(),
                Field = counterField
            };

            enumeratorType.AddField(counterField);
            enumeratorType.AddFieldInitializer(counterField, new LiteralNode(assemblyEmitter.TypeSystem.Int32, -1));

            var getEnumeratorMethod = new MethodEmitter(collectionType, "GetEnumerator", enumeratorTypeRef, MethodAttributes.Public);

            getEnumeratorMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ReturnNode()
                    {
                        Expression = new ObjectCreationNode()
                        {
                            ExpressionReturnType = enumeratorTypeRef,
                            Constructor = AssemblyRegistry.GetConstructor(assemblyEmitter, enumeratorTypeRef),
                            Args = new IExpressionNode[0]
                        }
                    }
                }
            });

            var moveNextMethod = new MethodEmitter(enumeratorType, "MoveNext", assemblyEmitter.TypeSystem.Boolean, MethodAttributes.Public);

            moveNextMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            BinaryOperatorType = BinaryOperatorNodeType.NotEquals,
                            ExpressionReturnType = assemblyEmitter.TypeSystem.Boolean,
                            LeftOperand = counterFieldNode,
                            RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 9)
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new IParserNode[]
                            {
                                new IncrementDecrementOperatorNode()
                                {
                                    IncrementDecrementType = IncrementDecrementOperatorType.PostIncrement,
                                    Operand = counterFieldNode
                                },
                                new ReturnNode()
                                {
                                    Expression = new LiteralNode(assemblyEmitter.TypeSystem.Boolean, true)
                                }
                            }
                        }
                    },
                    new ReturnNode()
                    {
                        Expression = new LiteralNode(assemblyEmitter.TypeSystem.Boolean, false)
                    }
                }
            });

            var getCurrentMethod = new MethodEmitter(enumeratorType, "get_Current", assemblyEmitter.TypeSystem.String, MethodAttributes.Public);

            getCurrentMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ReturnNode()
                    {
                        Expression = new MethodCallNode()
                        {
                            Function = new FunctionNode()
                            {
                                ObjectInstance = counterFieldNode,
                                Method = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, counterField.FieldType, "ToString", new TypeReference[0]),
                            },
                            Args = new IExpressionNode[0]
                        }
                    }
                }
            });

            var collectionCreation = new ValueCreationNode()
            {
                ExpressionReturnType = collectionTypeRef
            };

            var expectedOutput = Enumerable.Range(0, 10).Select(i => i + Environment.NewLine).Aggregate((x, y) => x + y);
            TestForEachLoopHelper(collectionCreation, assemblyEmitter.TypeSystem.String, expectedOutput);
        }

        #endregion

        /* Missing tests:
         * Functors with optional parameters and params parameters
         */

        #region Method Call tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateObject()
        {
            LocalVariableNode variable;

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    ConstructTypeEmitterInstance(out variable),
                    CallConsoleWriteLine(variable)
                }
            };

            var myType = typeEmitter.Get(assemblyEmitter);
            ExpectedOutput = myType.FullName;
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CallFunctionWithOptionalParameter()
        {
            var voidType = assemblyEmitter.TypeSystem.Void;
            var stringType = assemblyEmitter.TypeSystem.String;

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
                    CallConsoleWriteLine(new LiteralNode(stringType, "{0}: {1}"), new ParameterNode(neededParameter), new ParameterNode(optionalParameter))
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
                            new LiteralNode(stringType, "Hi")
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
                            new LiteralNode(stringType, "Hi"),
                            new LiteralNode(stringType, "NonOptional")
                        }
                    }
                }
            };

            ExpectedOutput = "Hi: Default value" + Environment.NewLine + "Hi: NonOptional";
            AssertSuccessByExecution();
        }

        #endregion

        #region Array tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateEmptyVector()
        {
            BodyCodeBlock = OutputEnumerable(new ArrayCreationNode()
            {
                ExpressionReturnType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1),
                Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 0) }
            });

            ExpectedOutput = "Total count: 0.";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateVector()
        {
            BodyCodeBlock = OutputEnumerable(new ArrayCreationNode()
            {
                ExpressionReturnType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1),
                Dimensions = new IExpressionNode[] { new LiteralNode(assemblyEmitter.TypeSystem.Int32, 5) }
            });

            ExpectedOutput = Enumerable.Repeat("0 ", 5).Aggregate((x, y) => x + y) + Environment.NewLine + "Total count: 5.";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateMultiDimentionalArray()
        {
            BodyCodeBlock = OutputEnumerable(new ArrayCreationNode()
            {
                ExpressionReturnType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 2),
                Dimensions = new IExpressionNode[] 
                {
                    new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2),
                    new LiteralNode(assemblyEmitter.TypeSystem.Int32, 3)
                }
            });

            ExpectedOutput = Enumerable.Repeat("0 ", 6).Aggregate((x, y) => x + y) + Environment.NewLine + "Total count: 6.";
            AssertSuccessByExecution();
        }

        private void TestCanEmit_CreateInitializedArrayHelper(ArrayType arrayType, int[] dimensions, LiteralNode[] initializer)
        {
            BodyCodeBlock = OutputEnumerable(new ArrayCreationNode()
            {
                ExpressionReturnType = arrayType,
                Dimensions = dimensions.Select(x => new LiteralNode(assemblyEmitter.TypeSystem.Int32, x)).ToArray<IExpressionNode>(),
                Initializer = initializer
            });

            ExpectedOutput = string.Format("{0}{1}Total count: {2}.", initializer.Select(x => x.Value.Value + " ").Aggregate((x, y) => x + y), Environment.NewLine, dimensions.Aggregate((x, y) => x * y));
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateAndInitializeVector()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1);
            var initializer = (new[] { 1, 2, 3, 4, 5 }).Select(x => new LiteralNode(assemblyEmitter.TypeSystem.Int32, x)).ToArray();
            TestCanEmit_CreateInitializedArrayHelper(arrayType, new[] { initializer.Length }, initializer);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateAndInitializeMultiDimentionalArray()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 2);
            var initializer = (new[] { 1, 2, 3, 4, 5, 6 }).Select(x => new LiteralNode(assemblyEmitter.TypeSystem.Int32, x)).ToArray();
            TestCanEmit_CreateInitializedArrayHelper(arrayType, new[] { 2, 3 }, initializer);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateAndInitializeStringVector()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 1);
            var initializer = (new[] { "abc", "def", "ghi", "jkl", "mno" }).Select(x => new LiteralNode(assemblyEmitter.TypeSystem.String, x)).ToArray();
            TestCanEmit_CreateInitializedArrayHelper(arrayType, new[] { 5 }, initializer);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateAndInitializeMultidimensionalStringArray()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 3);
            var arrayItems = new[]
            {
                "a", "b", "c", "d",
                "e", "f", "g", "h",
                "i", "j", "k", "l",
                
                "m", "n", "o", "p",
                "q", "r", "s", "t",
                "u", "v", "w", "x"
            };
            var initializer = arrayItems.Select(x => new LiteralNode(assemblyEmitter.TypeSystem.String, x)).ToArray();
            TestCanEmit_CreateInitializedArrayHelper(arrayType, new[] { 2, 3, 4 }, initializer);
        }

        private void TestCanEmit_GetElementHelper(ArrayType arrayType, int[] dimensions, LiteralNode[] values)
        {
            var arrayVariable = new VariableDefinition(arrayType);
            var arrayVariableNode = new LocalVariableNode(arrayVariable);
            var stringVariable = new VariableDefinition(assemblyEmitter.TypeSystem.String);
            var stringVariableNode = new LocalVariableNode(stringVariable);

            var bodyNodes = new List<IParserNode>()
            {
                new SymbolDeclarationNode()
                {
                    Variable = arrayVariable,
                    Initializer = new ArrayCreationNode()
                    {
                        ExpressionReturnType = arrayType,
                        Dimensions = dimensions.Select(x => new LiteralNode(assemblyEmitter.TypeSystem.Int32, x)).ToArray<IExpressionNode>(),
                        Initializer = values
                    }
                },
                new SymbolDeclarationNode()
                {
                    Variable = stringVariable
                }
            };

            BodyCodeBlock = new CodeBlockNode() { Nodes = bodyNodes };

            for (int i = 0; i < values.Length; i++)
            {
                var indices = new LiteralNode[arrayType.Rank];
                var indexSizes = 1;

                for (int j = arrayType.Rank - 1; j > -1; j--)
                {
                    indices[j] = new LiteralNode(assemblyEmitter.TypeSystem.Int32, (i / indexSizes) % dimensions[j]);
                    indexSizes *= dimensions[j];
                }

                bodyNodes.Add(new AssignmentOperatorNode()
                {
                    LeftOperand = stringVariableNode,
                    RightOperand = new BinaryOperatorNode()
                    {
                        ExpressionReturnType = assemblyEmitter.TypeSystem.String,
                        BinaryOperatorType = BinaryOperatorNodeType.Addition,
                        LeftOperand = stringVariableNode,
                        RightOperand = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = assemblyEmitter.TypeSystem.String,
                            BinaryOperatorType = BinaryOperatorNodeType.Addition,
                            LeftOperand = new LiteralNode(assemblyEmitter.TypeSystem.String, " "),
                            RightOperand = new ArrayAccessNode()
                            {
                                ExpressionReturnType = values[i].ExpressionReturnType,
                                ObjectInstance = arrayVariableNode,
                                Indices = indices
                            }
                        }
                    }
                });
            }

            bodyNodes.Add(CallConsoleWriteLine(stringVariableNode));

            ExpectedOutput = values.Select(v => (string)v.Value).Aggregate((x, y) => x + " " + y);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IntVectorGetElement()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1);
            var arrayItems = new int[] { 1337, 42, 314, 41597456 };

            TestCanEmit_GetElementHelper(arrayType, new[] { arrayItems.Length }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.Int32, value)).ToArray());
        }


        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StringVectorGetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 1);
            var arrayItems = new string[] { "Just", "why", "I", "ask", "it", "doesn't", "make", "much", "sense" };

            TestCanEmit_GetElementHelper(arrayType, new[] { arrayItems.Length }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.String, value)).ToArray());
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IntArrayGetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 3);
            var arrayItems = new int[]
            {
                15, 849, 1596,
                18, 15, 11,
                
                4, 5, 6,
                7849, 489, 654,
                
                48, 77, 777,
                111, 11, 123
            };

            TestCanEmit_GetElementHelper(arrayType, new[] { 3, 2, 3 }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.Int32, value)).ToArray());
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StringArrayGetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 3);
            var arrayItems = new string[]
            {
                "How", "many", "code", "must", "a", "man", "type", "in",
                "Before", "you", "can", "call", "him", "a", "man", "?",
                
                "How", "many", "C", "must", "a", "compiler", "process", "",
                "Before", "it", "can", "sleep", "in", "the", "RAM", "?",

                "How", "many", "times", "must", "a", "program", "crash", "",
                "before", "it", "is", "forever", "banned", "?", "", "",

                "The", "answer,", "my", "friend,", "is", "not", "a", "boolean",
                "The", "answer", "is", "not", "a", "boolean.", "", "",
                
                "How", "many", "times", "must", "a", "man", "read", "his source",
                "before", "he", "can", "see", "what", "is", "wrong", "?",

                "How", "many", "years", "must", "known", "bugs", "exist", "",
                "before", "you", "get", "an", "update", "for", "free", "?",

                "How", "many", "times", "can", "a drive", "move", "its", "head",
                "and", "pretend", "there's", "no", "data", "to", "read", "?",

                "The", "answer,", "my", "friend,", "is", "not", "a", "boolean",
                "The", "answer", "is", "not", "a", "boolean.", "", "",
            };

            TestCanEmit_GetElementHelper(arrayType, new[] { 8, 2, 8 }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.String, value)).ToArray());
        }

        private void TestCanEmit_SetElementHelper(ArrayType arrayType, int[] dimensions, LiteralNode[] values)
        {
            var arrayVariable = new VariableDefinition(arrayType);
            var arrayVariableNode = new LocalVariableNode(arrayVariable);

            var bodyNodes = new List<IParserNode>()
            {
                new SymbolDeclarationNode()
                {
                    Variable = arrayVariable,
                    Initializer = new ArrayCreationNode()
                    {
                        ExpressionReturnType = arrayType,
                        Dimensions = dimensions.Select(x => new LiteralNode(assemblyEmitter.TypeSystem.Int32, x)).ToArray<IExpressionNode>()
                    }
                }
            };

            BodyCodeBlock = new CodeBlockNode() { Nodes = bodyNodes };

            for (int i = 0; i < values.Length; i++)
            {
                var indices = new LiteralNode[arrayType.Rank];
                var indexSizes = 1;

                for (int j = arrayType.Rank - 1; j > -1; j--)
                {
                    indices[j] = new LiteralNode(assemblyEmitter.TypeSystem.Int32, (i / indexSizes) % dimensions[j]);
                    indexSizes *= dimensions[j];
                }

                bodyNodes.Add(new AssignmentOperatorNode()
                {
                    LeftOperand = new ArrayAccessNode()
                    {
                        ExpressionReturnType = values[i].ExpressionReturnType,
                        ObjectInstance = arrayVariableNode,
                        Indices = indices
                    },
                    RightOperand = values[i]
                });
            }

            bodyNodes.Add(OutputEnumerable(arrayVariableNode));

            ExpectedOutput = values.Select(v => (string)v.Value + " ").Aggregate((x, y) => x + y) + Environment.NewLine + string.Format("Total count: {0}.", values.Length);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IntVectorSetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 1);
            var arrayItems = new int[] { 3, 2, 1, 0 };

            TestCanEmit_SetElementHelper(arrayType, new[] { arrayItems.Length }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.Int32, value)).ToArray());
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StringVectorSetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 1);
            var arrayItems = new string[] { "456", "789", "bca", "dqawhixja", "1231231", "dajiusdok" };

            TestCanEmit_SetElementHelper(arrayType, new[] { arrayItems.Length }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.String, value)).ToArray());
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_IntArraySetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.Int32, 3);
            var arrayItems = new int[]
            {
                1, 2, 3,
                1, 3, 2,
                
                2, 1, 3,
                2, 3, 1,
                
                3, 1, 2,
                3, 2, 1,
            };

            TestCanEmit_SetElementHelper(arrayType, new[] { 3, 2, 3 }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.Int32, value)).ToArray());
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StringArraySetElements()
        {
            var arrayType = AssemblyRegistry.GetArrayType(assemblyEmitter.TypeSystem.String, 3);
            var arrayItems = new string[]
            {
                "a", "ą", "b",
                "c", "č", "d",
                "e", "ę", "ė",
                "f", "g", "h",
                               
                "i", "į", "y",
                "j", "k", "l",
                "m", "n", "o",
                "p", "r", "s",
                
                "š", "t", "u",
                "ų", "ū", "v",
                "z", "ž", "0",
                "1", "2", "3",
            };

            TestCanEmit_SetElementHelper(arrayType, new[] { 3, 4, 3 }, arrayItems.Select(value => new LiteralNode(assemblyEmitter.TypeSystem.String, value)).ToArray());
        }

        private void TestCanEmit_DateTimeArrayGetMinuteHelper(ArrayType arrayType)
        {
            var dateTimeType = AssemblyRegistry.FindType(assemblyEmitter, "System.DateTime");
            var minuteProperty = AssemblyRegistry.GetProperty(dateTimeType, "Minute");

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    CallConsoleWriteLine(new PropertyNode(assemblyEmitter, minuteProperty)
                    {
                        ObjectInstance = new ArrayAccessNode()
                        {
                            ExpressionReturnType = dateTimeType,
                            ObjectInstance = new ArrayCreationNode()
                            {
                                ExpressionReturnType = arrayType,
                                Dimensions = Enumerable.Repeat(1, arrayType.Rank).Select(i => new LiteralNode(assemblyEmitter.TypeSystem.Int32, i)).ToArray(),
                                Initializer = new IExpressionNode[]
                                {
                                    new ObjectCreationNode()
                                    {
                                        ExpressionReturnType = dateTimeType,
                                        Constructor = AssemblyRegistry.GetCompatibleConstructor(assemblyEmitter, dateTimeType, Enumerable.Repeat(assemblyEmitter.TypeSystem.Int32, 6).ToArray()),
                                        Args = new IExpressionNode[]
                                        {
                                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 1),
                                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 2),
                                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 3),
                                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 4),
                                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 5),
                                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 6)
                                        }
                                    }
                                }
                            },
                            Indices = Enumerable.Repeat(0, arrayType.Rank).Select(i => new LiteralNode(assemblyEmitter.TypeSystem.Int32, i)).ToArray()
                        },
                        Property = minuteProperty
                    })
                }
            };

            ExpectedOutput = "5";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_DateTimeVectorGetMinute()
        {
            var dateTimeType = AssemblyRegistry.FindType(assemblyEmitter, "System.DateTime");
            var arrayType = AssemblyRegistry.GetArrayType(dateTimeType, 1);
            TestCanEmit_DateTimeArrayGetMinuteHelper(arrayType);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_DateTimeArrayGetMinute()
        {
            var dateTimeType = AssemblyRegistry.FindType(assemblyEmitter, "System.DateTime");
            var arrayType = AssemblyRegistry.GetArrayType(dateTimeType, 3);
            TestCanEmit_DateTimeArrayGetMinuteHelper(arrayType);
        }

        private PropertyDefinition SetupIndexedProperty(TypeReference elementType, int rank, IReadOnlyList<int> dimensions, IReadOnlyList<IExpressionNode> initializer = null)
        {
            var arrayType = AssemblyRegistry.GetArrayType(elementType, rank);
            var backingArrayField = new FieldDefinition("__item", FieldAttributes.Private, arrayType);

            typeEmitter.AddField(backingArrayField);
            typeEmitter.AddFieldInitializer(backingArrayField, new ArrayCreationNode()
                {
                    ExpressionReturnType = arrayType,
                    Dimensions = dimensions.Select(dim => new LiteralNode(assemblyEmitter.TypeSystem.Int32, dim)).ToArray(),
                    Initializer = initializer
                });

            var property = new PropertyDefinition("Item", PropertyAttributes.None, elementType);
            
            var indices = new List<IExpressionNode>();            
            var arrayAccessNode = new ArrayAccessNode()
            {
                ExpressionReturnType = elementType,
                ObjectInstance = new FieldNode()
                {
                    ObjectInstance = new ThisNode(),
                    Field = backingArrayField
                },
                Indices = indices
            };

            var getMethod = new MethodEmitter(typeEmitter, "get_Item", elementType, MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.SpecialName);
            property.GetMethod = getMethod.Get().Resolve();

            var setMethod = new MethodEmitter(typeEmitter, "set_Item", assemblyEmitter.TypeSystem.Void, MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.SpecialName);
            property.SetMethod = setMethod.Get().Resolve();

            for (int i = 0; i < rank; i++)
            {
                var parameter = new ParameterDefinition(assemblyEmitter.TypeSystem.Int32);
                indices.Add(new ParameterNode(parameter));

                getMethod.AddArgument(parameter);
                setMethod.AddArgument(parameter);
            }

            var setParameterValue = new ParameterDefinition("value", ParameterAttributes.None, elementType);
            setMethod.AddArgument(setParameterValue);
            
            getMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ReturnNode()
                    {
                        Expression = arrayAccessNode
                    }
                }
            });

            setMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = arrayAccessNode,
                        RightOperand = new ParameterNode(setParameterValue)
                    }
                }
            });

            typeEmitter.AddProperty(property);
            return property;
        }

        private void TestCanEmit_IndexOperatorGetterHelper(PropertyDefinition property, int[] dimensions, IConvertible[] values, List<IParserNode> nodes, IExpressionNode thisObjectInstance)
        {
            var rank = dimensions.Length;

            for (int i = 0; i < values.Length; i++)
            {
                var indices = new LiteralNode[rank];
                var indexSizes = 1;

                for (int j = rank - 1; j > -1; j--)
                {
                    indices[j] = new LiteralNode(assemblyEmitter.TypeSystem.Int32, (i / indexSizes) % dimensions[j]);
                    indexSizes *= dimensions[j];
                }

                nodes.Add(CallConsoleWriteLine(new IndexOperatorNode()
                    {
                        ObjectInstance = thisObjectInstance,
                        Property = property,
                        Indices = indices
                    }));
            }

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = nodes
            };

            ExpectedOutput = values.Select(value => value.ToString() + Environment.NewLine).Aggregate((x, y) => x + y);
            AssertSuccessByExecution();
        }

        private void TestCanEmit_IndexOperatorGetterHelper(TypeReference elementType, int[] dimensions, IConvertible[] values)
        {
            var property = SetupIndexedProperty(elementType, dimensions.Length, dimensions, values.Select(value => new LiteralNode(elementType, value)).ToArray());
            var nodes = new List<IParserNode>();

            LocalVariableNode objectInstance;
            nodes.Add(ConstructTypeEmitterInstance(out objectInstance));
            TestCanEmit_IndexOperatorGetterHelper(property, dimensions, values, nodes, objectInstance);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_SingleDimentionalIntIndexOperatorGetter()
        {
            TestCanEmit_IndexOperatorGetterHelper(assemblyEmitter.TypeSystem.Int32, new int[] { 5 }, new IConvertible[]
                {
                    12,
                    34,
                    56,
                    78,
                    90
                });
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_MultiDimentionalIntIndexOperatorGetter()
        {
            TestCanEmit_IndexOperatorGetterHelper(assemblyEmitter.TypeSystem.Int32, new int[] { 5, 2 }, new IConvertible[]
                {
                    1, 2,
                    3, 4,
                    5, 6,
                    7, 8,
                    9, 0
                });
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_SingleDimentionalStringIndexOperatorGetter()
        {
            TestCanEmit_IndexOperatorGetterHelper(assemblyEmitter.TypeSystem.String, new int[] { 5 }, new IConvertible[]
                {
                    "ab",
                    "cd",
                    "ef",
                    "gh",
                    "ij"
                });
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_MultiDimentionalStringIndexOperatorGetter()
        {
            TestCanEmit_IndexOperatorGetterHelper(assemblyEmitter.TypeSystem.String, new int[] { 5, 2 }, new IConvertible[]
                {
                    "a", "b",
                    "c", "d",
                    "e", "f",
                    "g", "h",
                    "i", "j"
                });
        }

        private void TestCanEmit_IndexOperatorSetterHelper(TypeReference elementType, int[] dimensions, IConvertible[] values)
        {
            LocalVariableNode objectInstance;
            var rank = dimensions.Length;
            var property = SetupIndexedProperty(elementType, dimensions.Length, dimensions);
            var nodes = new List<IParserNode>();
            
            nodes.Add(ConstructTypeEmitterInstance(out objectInstance));

            for (int i = 0; i < values.Length; i++)
            {
                var indices = new LiteralNode[rank];
                var indexSizes = 1;

                for (int j = rank - 1; j > -1; j--)
                {
                    indices[j] = new LiteralNode(assemblyEmitter.TypeSystem.Int32, (i / indexSizes) % dimensions[j]);
                    indexSizes *= dimensions[j];
                }

                nodes.Add(new AssignmentOperatorNode()
                {
                    LeftOperand = new IndexOperatorNode()
                    {
                        ObjectInstance = objectInstance,
                        Property = property,
                        Indices = indices
                    },
                    RightOperand = new LiteralNode(elementType, values[i])
                });
            }
            
            TestCanEmit_IndexOperatorGetterHelper(property, dimensions, values, nodes, objectInstance);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_SingleDimentionalIntIndexOperatorSetter()
        {
            TestCanEmit_IndexOperatorSetterHelper(assemblyEmitter.TypeSystem.Int32, new int[] { 5 }, new IConvertible[]
                {
                    12,
                    34,
                    56,
                    78,
                    90
                });
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_MultiDimentionalIntIndexOperatorSetter()
        {
            TestCanEmit_IndexOperatorSetterHelper(assemblyEmitter.TypeSystem.Int32, new int[] { 5, 2 }, new IConvertible[]
                {
                    1, 2,
                    3, 4,
                    5, 6,
                    7, 8,
                    9, 0
                });
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_SingleDimentionalStringIndexOperatorSetter()
        {
            TestCanEmit_IndexOperatorSetterHelper(assemblyEmitter.TypeSystem.String, new int[] { 5 }, new IConvertible[]
                {
                    "ab",
                    "cd",
                    "ef",
                    "gh",
                    "ij"
                });
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_MultiDimentionalStringIndexOperatorSetter()
        {
            TestCanEmit_IndexOperatorSetterHelper(assemblyEmitter.TypeSystem.String, new int[] { 5, 2 }, new IConvertible[]
                {
                    "a", "b",
                    "c", "d",
                    "e", "f",
                    "g", "h",
                    "i", "j"
                });
        }

        #endregion

        #region Value Creation tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ValueCreation()
        {
            var localVariable = new VariableDefinition(assemblyEmitter.TypeSystem.Int32);
            var localVariableNode = new LocalVariableNode(localVariable);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable,
                        Initializer = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 5)
                    },
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = localVariableNode,
                        RightOperand = new ValueCreationNode(assemblyEmitter.TypeSystem.Int32)
                    },
                    CallConsoleWriteLine(localVariableNode)
                }
            };

            ExpectedOutput = "0";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ValueCreationOfDifferentType()
        {
            var localVariable = new VariableDefinition(assemblyEmitter.TypeSystem.Byte);
            var localVariableNode = new LocalVariableNode(localVariable);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable,
                        Initializer = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 22)
                    },
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = localVariableNode,
                        RightOperand = new ValueCreationNode(assemblyEmitter.TypeSystem.Int64)
                    },
                    CallConsoleWriteLine(localVariableNode)
                }
            };

            ExpectedOutput = "0";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ValueCreationAssignmentToField()
        {
            var field = new FieldDefinition("_testField", FieldAttributes.Private, assemblyEmitter.TypeSystem.Int32);

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, new LiteralNode(assemblyEmitter.TypeSystem.Int32, 12));

            var thisVariable = new VariableDefinition(typeEmitter.Get(assemblyEmitter));
            var thisVariableNode = new LocalVariableNode(thisVariable);

            var fieldNode = new FieldNode()
            {
                ObjectInstance = thisVariableNode,
                Field = field
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = thisVariable,
                        Initializer = ConstructTypeEmitterInstance()
                    },
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = fieldNode,
                        RightOperand = new ValueCreationNode(assemblyEmitter.TypeSystem.Int32)
                    },
                    CallConsoleWriteLine(fieldNode)
                }
            };

            ExpectedOutput = "0";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ValueCreationPassToMethod()
        {
            var targetMethod = new MethodEmitter(typeEmitter, "PrintInt", assemblyEmitter.TypeSystem.Void, MethodAttributes.Private | MethodAttributes.Static);
            var parameter = new ParameterDefinition(assemblyEmitter.TypeSystem.Int32);

            targetMethod.AddArgument(parameter);
            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    CallConsoleWriteLine(new ParameterNode(parameter))
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = targetMethod.Get()
                        },
                        Args = new IExpressionNode[]
                        {
                            new ValueCreationNode()
                            {
                                ExpressionReturnType = assemblyEmitter.TypeSystem.Int32
                            }
                        }
                    }
                }
            };

            ExpectedOutput = "0";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_ValueCreationAssignmentToFunctionParameter()
        {
            var targetMethod = new MethodEmitter(typeEmitter, "PrintDefaultInt", assemblyEmitter.TypeSystem.Void, MethodAttributes.Private | MethodAttributes.Static);
            var parameter = new ParameterDefinition(assemblyEmitter.TypeSystem.Int32);
            var parameterNode = new ParameterNode(parameter);

            targetMethod.AddArgument(parameter);
            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = parameterNode,
                        RightOperand = new ValueCreationNode(parameterNode.ExpressionReturnType)
                    },
                    CallConsoleWriteLine(parameterNode)
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = targetMethod.Get()
                        },
                        Args = new IExpressionNode[]
                        {
                            new LiteralNode(assemblyEmitter.TypeSystem.Int32, 125637)
                        }
                    }
                }
            };

            ExpectedOutput = "0";
            AssertSuccessByExecution();
        }

        #endregion

        #region Cast tests

        private void TestCanEmit_CastHelper(IExpressionNode emitValueNode, string expectedOutput, TypeReference castFrom, TypeReference castTo)
        {
            var createValueMethod = new MethodEmitter(typeEmitter, "CreateValue", castFrom, MethodAttributes.Static | MethodAttributes.Private);

            createValueMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new ReturnNode()
                    {
                        Expression = emitValueNode
                    }
                }
            });

            var printValueMethod = new MethodEmitter(typeEmitter, "PrintValue", assemblyEmitter.TypeSystem.Void, MethodAttributes.Static | MethodAttributes.Private);
            var argument = printValueMethod.AddArgument(castTo, "arg");

            printValueMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    CallConsoleWriteLine(new ParameterNode(argument))
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = printValueMethod.Get()
                        },
                        Args = new IExpressionNode[]
                        {
                            new CastNode()
                            {
                                ExpressionReturnType = castTo,
                                TargetExpression = new MethodCallNode()
                                {
                                    Function = new FunctionNode()
                                    {
                                        Method = createValueMethod.Get()
                                    },
                                    Args = new IExpressionNode[0]
                                }
                            }
                        }
                    }
                }
            };

            ExpectedOutput = expectedOutput;
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CastObjectToString()
        {
            var expectedOutput = "Hello";
            var valueExpression = new LiteralNode(assemblyEmitter.TypeSystem.String, expectedOutput);

            TestCanEmit_CastHelper(valueExpression, expectedOutput, assemblyEmitter.TypeSystem.Object, assemblyEmitter.TypeSystem.String);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CastObjectToInt()
        {
            var intValue = 333;
            var valueExpression = new LiteralNode(assemblyEmitter.TypeSystem.Int32, intValue);

            TestCanEmit_CastHelper(valueExpression, intValue.ToString(), assemblyEmitter.TypeSystem.Object, assemblyEmitter.TypeSystem.Int32);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CastIntToObject()
        {
            var intValue = 333;
            var valueExpression = new LiteralNode(assemblyEmitter.TypeSystem.Int32, intValue);

            TestCanEmit_CastHelper(valueExpression, intValue.ToString(), assemblyEmitter.TypeSystem.Int32, assemblyEmitter.TypeSystem.Object);
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CastIntToShort()
        {
            const int kIntValue = 14548916;
            
            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new IParserNode[]
                {
                    CallConsoleWriteLine(new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            ObjectInstance = new CastNode()
                            {
                                ExpressionReturnType = assemblyEmitter.TypeSystem.Int16,
                                TargetExpression = new LiteralNode(assemblyEmitter.TypeSystem.Int32, kIntValue)
                            },
                            Method = AssemblyRegistry.GetMethod(assemblyEmitter, assemblyEmitter.TypeSystem.Object, "ToString")
                        },
                        Args = new IExpressionNode[0]
                    })
                }
            };

            ExpectedOutput = unchecked((short)kIntValue).ToString();
            AssertSuccessByExecution();
        }

        #endregion
    }
}
