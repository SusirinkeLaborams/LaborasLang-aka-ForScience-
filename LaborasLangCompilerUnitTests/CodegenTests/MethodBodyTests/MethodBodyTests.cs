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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeSystem.Void,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 1)
                        }
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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new LiteralNode(assemblyEmitter.TypeSystem.Int32, 42)
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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeSystem.Void,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(backingField),
                            RightOperand = new ParameterNode(argument)
                        }
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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeSystem.Void,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new PropertyNode(property),
                            RightOperand = new LocalVariableNode(localVariable)
                        }
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
                    new LiteralNode(assemblyEmitter.TypeSystem.String, "Test2")
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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeSystem.Void,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new ParameterNode(argument),
                            RightOperand = new PropertyNode(property)
                        }
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
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeSystem.Void,
                        Operand = assignmentNode
                    },
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
                                Constructor = AssemblyRegistry.GetMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter), ".ctor"),
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
        public void TestCanEmit_Negation_BinaryNot_Increment_Decrement()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;

            var variableDefs = new List<VariableDefinition>();

            for(int i = 0; i < 7; i++)
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

            nodes.Add(CallConsoleWriteLine( 
                new LiteralNode(stringType, "Results: \r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n"),
                variables[0],
                variables[1],
                variables[2],
                variables[3],
                variables[4],
                variables[5],
                variables[6]
            ));

            ExpectedOutput = "Results: \r\n5\r\n-6\r\n-5\r\n5\r\n4\r\n4\r\n5\r\n";
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
                                    new UnaryOperatorNode()
                                    {
                                        ExpressionReturnType = intType,
                                        UnaryOperatorType = UnaryOperatorNodeType.PostIncrement,
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
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode(field),
                            RightOperand = new LiteralNode(boolType, true)
                        }
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

        #endregion

        /* Missing tests:
         * Functors with optional parameters and params parameters
         */

        #region Method Call tests

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CreateObject()
        {
            typeEmitter.AddDefaultConstructor();
            var myType = typeEmitter.Get(assemblyEmitter);

            var variable = new VariableDefinition("myInstance", myType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = variable,
                        Initializer = new ObjectCreationNode()
                        {
                            ExpressionReturnType = myType,
                            Constructor = AssemblyRegistry.GetMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter), ".ctor"),
                            Args = new List<IExpressionNode>()
                        }
                    },
                    CallConsoleWriteLine(new LocalVariableNode(variable))
                }
            };

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
    }
}
