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
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.CodegenTests.MethodBodyTests
{
    [TestClass]
    public class FunctorTests : CodegenTestBase
    {
        public FunctorTests()
        {
        }

        internal FunctorTests(AssemblyEmitter assemblyEmitter, string className, bool bulkTesting) :
            base(assemblyEmitter, className, bulkTesting)
        {
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctorDefinition()
        {
            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new[] { assemblyEmitter.TypeSystem.String });
            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, consoleWriteLine);
            var localVariable = new VariableDefinition(functorType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable,
                        Initializer = new FunctionNode()
                        {
                            ExpressionReturnType = functorType,
                            Method = consoleWriteLine
                        }                        
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = consoleWriteLine.ReturnType,
                        Function = new LocalVariableNode(localVariable),
                        Args = new[]
                        {
                            new LiteralNode(assemblyEmitter.TypeSystem.String, "Hello, world!")
                        }
                    }
                }
            };

            ExpectedOutput = "Hello, world!";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctorWithReturnTypeAndArguments()
        {
            const int kReturnValue = 95413;
            const bool kArg1 = true;
            const float kArg2 = 489.14f;
            const string kArg3 = "A string value";

            var intType = assemblyEmitter.TypeSystem.Int32;
            var boolType = assemblyEmitter.TypeSystem.Boolean;
            var floatType = assemblyEmitter.TypeSystem.Single;
            var stringType = assemblyEmitter.TypeSystem.String;

            var targetMethod = EmitMethodToOutputArgs(new LiteralNode(intType, kReturnValue), boolType, floatType, stringType);
            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, targetMethod);
            var localVariable = new VariableDefinition(functorType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        Variable = localVariable,
                        Initializer = new FunctionNode()
                        {
                            ExpressionReturnType = functorType,
                            Method = targetMethod
                        }
                    },
                    CallConsoleWriteLine(
                        new MethodCallNode()
                        {
                            ExpressionReturnType = intType,
                            Function = new LocalVariableNode(localVariable),
                            Args = new[]
                            {
                                new LiteralNode(boolType, kArg1),
                                new LiteralNode(floatType, kArg2),
                                new LiteralNode(stringType, kArg3)
                            }
                        })
                }
            };

            ExpectedOutput = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", kArg1, kArg2, kArg3, kReturnValue);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctionAssignmentToFunctorWithoutArgs()
        {
            var voidType = assemblyEmitter.TypeSystem.Void;

            var targetMethod = new MethodEmitter(typeEmitter, "TargetMethod", voidType);

            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeSystem.String, "TargetMethod was called"))
                }       
            });

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, targetMethod.Get());
            var field = new FieldDefinition("myFunction", FieldAttributes.Public, functorType);

            var initializer = new FunctionNode()
            {
                Method = targetMethod.Get(),
                ObjectInstance = new ThisNode()
                {
                    ExpressionReturnType = typeEmitter.Get(assemblyEmitter)
                },
                ExpressionReturnType = functorType
            };

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, initializer);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FieldNode()
                        {
                            Field = field,
                            ObjectInstance = ConstructTypeEmitterInstance()
                        },
                        Args = new IExpressionNode[0]
                    }
                }
            };

            ExpectedOutput = "TargetMethod was called";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctionAssignmentToFunctorWithArgs()
        {
            const int kReturnValue = 5;
            const double kArg1 = 12.3;
            const float kArg2 = -1.222f;

            var intType = assemblyEmitter.TypeSystem.Int32;
            var doubleType = assemblyEmitter.TypeSystem.Double;
            var floatType = assemblyEmitter.TypeSystem.Single;

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType,
                new List<TypeReference>()
                {
                    doubleType,
                    floatType
                });

            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);
            var initializer = new FunctionNode()
            {
                Method = EmitMethodToOutputArgs(new LiteralNode(intType, kReturnValue), doubleType, floatType),
                ExpressionReturnType = functorType
            };

            GenerateBodyToOutputExpression(new MethodCallNode()
                {
                    ExpressionReturnType = intType,
                    Function = new FieldNode(field),
                    Args = new List<IExpressionNode>()
                    {
                        new LiteralNode(doubleType, kArg1),
                        new LiteralNode(floatType, kArg2)
                    }
                });

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, initializer);

            ExpectedOutput = string.Format("{0}\r\n{1}\r\n{2}", kArg1, kArg2, kReturnValue);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctorAssignmentToDelegate()
        {
            const int kArg1 = 123;
            const string kArg2 = "fghbsajdnkmf";

            var voidType = assemblyEmitter.TypeSystem.Void;
            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;
            var arguments = new TypeReference[]
                {
                    intType,
                    stringType
                };

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, voidType, arguments);            
            var functorField = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);
            typeEmitter.AddField(functorField);

            typeEmitter.AddFieldInitializer(functorField, new FunctionNode()
                {
                    Method = EmitMethodToOutputArgs(null, arguments),
                    ExpressionReturnType = functorType
                });

            var declaringType = (TypeDefinition)typeEmitter.Get(assemblyEmitter);
            var delegateType = DelegateEmitter.Create(assemblyEmitter, declaringType, voidType, arguments);
            declaringType.NestedTypes.Add(delegateType);

            var delegateField = new FieldDefinition("myDelegate", FieldAttributes.Public | FieldAttributes.Static, delegateType);
            typeEmitter.AddField(delegateField);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(delegateField),
                        RightOperand = new FieldNode(functorField)
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FieldNode(delegateField),
                        Args = new IExpressionNode[]
                        {
                            new LiteralNode(intType, kArg1),
                            new LiteralNode(stringType, kArg2)
                        }
                    }
                }
            };

            ExpectedOutput = string.Format("{0}\r\n{1}", kArg1, kArg2);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctionAssignmentToDelegate()
        {
            var voidType = assemblyEmitter.TypeSystem.Void;

            var targetMethod = new MethodEmitter(typeEmitter, "TargetMethod", voidType, MethodAttributes.Private | MethodAttributes.Static);
            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(new LiteralNode(assemblyEmitter.TypeSystem.String, "Inside target method"))
                }
            });

            var declaringType = (TypeDefinition)typeEmitter.Get(assemblyEmitter);
            var delegateType = DelegateEmitter.Create(assemblyEmitter, "MyDelegate", declaringType, voidType, new TypeReference[0]);
            declaringType.NestedTypes.Add(delegateType);

            var delegateField = new FieldDefinition("myDelegate", FieldAttributes.Public | FieldAttributes.Static, delegateType);
            typeEmitter.AddField(delegateField);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(delegateField),
                        RightOperand = new FunctionNode()
                        {
                            ExpressionReturnType = delegateType,
                            Method = targetMethod.Get()
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FieldNode(delegateField),
                        Args = new IExpressionNode[0]
                    }
                }
            };

            ExpectedOutput = "Inside target method";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_CallFunctor_PassReturnValueAsArgument()
        {
            const int kReturnValue = 75231;
            const string kArg1 = "Str";
            const float kArg2 = 3.5f;

            var voidType = assemblyEmitter.TypeSystem.Void;
            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;
            var floatType = assemblyEmitter.TypeSystem.Single;

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType, new List<TypeReference>()
                {
                    stringType,
                    floatType
                });

            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, new FunctionNode()
            {
                ExpressionReturnType = functorType,
                Method = EmitMethodToOutputArgs(new LiteralNode(intType, kReturnValue), stringType, floatType)
            }); 

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    CallConsoleWriteLine(
                        new MethodCallNode()
                        {
                            ExpressionReturnType = intType,
                            Function = new FieldNode(field),
                            Args = new List<IExpressionNode>()
                            {
                                new MethodCallNode()
                                {
                                    Args = new IExpressionNode[0],
                                    Function = new FunctionNode()
                                    {
                                        ExpressionReturnType = stringType,
                                        Method = EmitMethodToOutputArgs(new LiteralNode(stringType, kArg1))
                                    },
                                    ExpressionReturnType = stringType
                                },
                                new LiteralNode(floatType, kArg2)
                            }
                        })
                }
            };

            ExpectedOutput = string.Format("{0}\r\n{1}\r\n{2}", kArg1, kArg2, kReturnValue);
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_FunctorPropertyAssignmentToDelegate()
        {
            const int kArg1 = 485613;
            const string kArg2 = "FASD4FSAD14asdf";

            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;

            var voidType = assemblyEmitter.TypeSystem.Void;
            var arguments = new TypeReference[]
                {
                    intType,
                    stringType
                };

            #region Functor Property Setup

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, voidType, arguments);

            var functorField = new FieldDefinition("myFunction_BackingField", FieldAttributes.Public | FieldAttributes.Static, functorType);
            typeEmitter.AddField(functorField);

            #region Setter

            var functorSetter = new MethodEmitter(typeEmitter, "set_MyFunction", voidType, MethodAttributes.Public | MethodAttributes.Static);
            var functorSetterArgument = functorSetter.AddArgument(functorType, "value");

            functorSetter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(functorField),
                        RightOperand = new ParameterNode(functorSetterArgument)
                    }
                }
            });

            #endregion

            #region Getter

            var functorGetter = new MethodEmitter(typeEmitter, "get_MyFunction", functorType, MethodAttributes.Public | MethodAttributes.Static);
            functorGetter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new FieldNode(functorField)
                    }
                }
            });

            #endregion

            var functorProperty = new PropertyDefinition("MyFunction", PropertyAttributes.None, functorType);
            functorProperty.SetMethod = functorSetter.Get().Resolve();
            functorProperty.GetMethod = functorGetter.Get().Resolve();
            typeEmitter.AddProperty(functorProperty);

            #endregion

            #region Delegate Property setup

            var declaringType = (TypeDefinition)typeEmitter.Get(assemblyEmitter);
            var delegateType = DelegateEmitter.Create(assemblyEmitter, declaringType, voidType, arguments);
            declaringType.NestedTypes.Add(delegateType);

            var delegateField = new FieldDefinition("myDelegate_BackingField", FieldAttributes.Private | FieldAttributes.Static, delegateType);
            typeEmitter.AddField(delegateField);

            #region Setter

            var delegateSetter = new MethodEmitter(typeEmitter, "set_MyDelegate", voidType, MethodAttributes.Public | MethodAttributes.Static);
            var delegateSetterArgument = delegateSetter.AddArgument(delegateType, "value");

            delegateSetter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new FieldNode(delegateField),
                        RightOperand = new ParameterNode(delegateSetterArgument)
                    }
                }
            });

            #endregion

            #region Getter

            var delegateGetter = new MethodEmitter(typeEmitter, "get_MyDelegate", delegateType, MethodAttributes.Public | MethodAttributes.Static);
            delegateGetter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new FieldNode(delegateField)
                    }
                }
            });

            #endregion

            var delegateProperty = new PropertyDefinition("MyDelegate", PropertyAttributes.None, delegateType);
            delegateProperty.SetMethod = delegateSetter.Get().Resolve();
            delegateProperty.GetMethod = delegateGetter.Get().Resolve();
            typeEmitter.AddProperty(delegateProperty);

            #endregion

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new PropertyNode(assemblyEmitter, functorProperty),
                        RightOperand = new FunctionNode()
                        {
                            ExpressionReturnType = functorType,
                            Method = EmitMethodToOutputArgs(null, arguments)
                        }
                    },
                    
                    new AssignmentOperatorNode()
                    {
                        LeftOperand = new PropertyNode(assemblyEmitter, delegateProperty),
                        RightOperand = new PropertyNode(assemblyEmitter, functorProperty)
                    },

                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new PropertyNode(assemblyEmitter, delegateProperty),
                        Args = new IExpressionNode[]
                        {
                            new LiteralNode(intType, kArg1),
                            new LiteralNode(stringType, kArg2)
                        }
                    }
                }
            };

            ExpectedOutput = string.Format("{0}\r\n{1}", kArg1, kArg2);
            AssertSuccessByExecution();
        }

        private TypeReference GetFunctorType(TypeReference returnType, params TypeReference[] args)
        {
            return AssemblyRegistry.GetFunctorType(assemblyEmitter, returnType, args);
        }

        [TestMethod, TestCategory("Misc IL Tests"), TestCategory("All Tests")]
        public void Test_FunctorNamesDoNotClash()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;
            var stringType = assemblyEmitter.TypeSystem.String;
            var floatType = assemblyEmitter.TypeSystem.Single;
            var boolType = assemblyEmitter.TypeSystem.Boolean;

            var types = new[]
            {
                GetFunctorType(intType, stringType, floatType, boolType),
                GetFunctorType(floatType, boolType),
                GetFunctorType(intType, stringType, GetFunctorType(floatType, boolType)),
                GetFunctorType(stringType, floatType),
                GetFunctorType(intType, GetFunctorType(stringType, floatType), boolType),
                GetFunctorType(intType, stringType),
                GetFunctorType(GetFunctorType(intType, stringType), floatType, boolType),
                GetFunctorType(floatType, boolType),
                GetFunctorType(GetFunctorType(intType, stringType), GetFunctorType(floatType, boolType)),
                GetFunctorType(stringType, floatType, boolType),
                GetFunctorType(intType, GetFunctorType(stringType, floatType, boolType)),
                GetFunctorType(intType, stringType, floatType),
                GetFunctorType(GetFunctorType(intType, stringType, floatType), boolType),
                GetFunctorType(intType, stringType, floatType, boolType),
                GetFunctorType(GetFunctorType(intType, stringType, floatType, boolType))
            };

            foreach (var type in types)
                assemblyEmitter.AddTypeIfNotAdded(type.Resolve());

            ExpectedILFilePath = "Test_FunctorNamesDoNotClash.il";
            AssertSuccessByILComparison();
        }
    }
}
