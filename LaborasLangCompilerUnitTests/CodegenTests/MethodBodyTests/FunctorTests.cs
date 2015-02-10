using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.CodegenTests.MethodBodyTests
{
    [TestClass]
    public class FunctorTests : ILTestBase
    {
        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_FunctorDefinition()
        {
            FunctorBaseTypeEmitter.Create(assemblyEmitter, assemblyEmitter.TypeToTypeReference(typeof(void)), new List<TypeReference>());
            FunctorImplementationTypeEmitter.Create(assemblyEmitter, typeEmitter.Get(assemblyEmitter), AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new string[] { }));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_FunctorDefinition.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_FunctorWithReturnTypeAndArguments()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));

            var targetMethod = new MethodEmitter(typeEmitter, "MethodWithArgs", intType, MethodAttributes.Static | MethodAttributes.Private);
            targetMethod.AddArgument(boolType, "boolArg");
            targetMethod.AddArgument(floatType, "floatArg");
            targetMethod.AddArgument(stringType, "stringArg");

            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new LiteralNode()
                        {
                            ExpressionReturnType = intType,
                            Value = -1
                        }
                    }
                }
            });

            FunctorImplementationTypeEmitter.Create(assemblyEmitter, typeEmitter.Get(assemblyEmitter), targetMethod.Get());

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_FunctorWithReturnTypeAndArguments.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_FunctionAssignmentToFunctorWithoutArgs()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType, new List<TypeReference>());
            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);

            var initializer = new FunctionNode()
            {
                Method = methodEmitter.Get(),
                ExpressionReturnType = functorType
            };

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, initializer);

            ExpectedILFilePath = "TestCanEmit_FunctionAssignmentToFunctorWithoutArgs.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_FunctionAssignmentToFunctorWithArgs()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, intType,
                new List<TypeReference>()
                {
                    doubleType,
                    floatType
                });
            var field = new FieldDefinition("myFunction", FieldAttributes.Public | FieldAttributes.Static, functorType);

            var targetMethod = new MethodEmitter(typeEmitter, "FunctionWithArgs", intType, MethodAttributes.Private | MethodAttributes.Static);
            targetMethod.AddArgument(doubleType, "d");
            targetMethod.AddArgument(floatType, "f");

            targetMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new LiteralNode()
                        {
                            ExpressionReturnType = intType,
                            Value = 5
                        }
                    }
                }
            });

            var initializer = new FunctionNode()
            {
                Method = targetMethod.Get(),
                ExpressionReturnType = functorType
            };

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, initializer);

            ExpectedILFilePath = "TestCanEmit_FunctionAssignmentToFunctorWithArgs.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
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
                        ExpressionReturnType = voidType,
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
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_FunctionAssignmentToDelegate()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var myMethod = methodEmitter.Get();
            var methodReturnType = myMethod.ReturnType;
            var methodArguments = myMethod.Parameters.Select(parameter => parameter.ParameterType).ToList();

            var declaringType = (TypeDefinition)typeEmitter.Get(assemblyEmitter);
            var delegateType = DelegateEmitter.Create(assemblyEmitter, "MyDelegate", declaringType, methodReturnType, methodArguments);
            declaringType.NestedTypes.Add(delegateType);

            var delegateField = new FieldDefinition("myDelegate", FieldAttributes.Public | FieldAttributes.Static, delegateType);
            typeEmitter.AddField(delegateField);

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
                            LeftOperand = new FieldNode()
                            {
                                Field = delegateField
                            },
                            RightOperand = new FunctionNode()
                            {
                                ExpressionReturnType = delegateType,
                                Method = myMethod
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_FunctionAssignmentToDelegate.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
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
                        ExpressionReturnType = stringType,
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
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new MethodCallNode()
                        {
                            Function = new FieldNode()
                            {
                                Field = field
                            },
                            Args = new List<IExpressionNode>()
                            {
                                new MethodCallNode()
                                {
                                    Args = new List<IExpressionNode>(),
                                    Function = new FunctionNode()
                                    {
                                        ExpressionReturnType = stringType,
                                        Method = getFirstArgumentMethod.Get()
                                    },
                                    ExpressionReturnType = stringType
                                },
                                new LiteralNode()
                                {
                                    ExpressionReturnType = floatType,
                                    Value = 3.5f
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CallFunctor_PassReturnValueAsArgument.il";
            AssertSuccessByILComparison();
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void TestCanEmit_FunctorPropertyAssignmentToDelegate()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var arguments = new List<TypeReference>()
                {
                    assemblyEmitter.TypeToTypeReference(typeof(int)),
                    assemblyEmitter.TypeToTypeReference(typeof(string))
                };

            #region Functor Property Setup

            var functorType = AssemblyRegistry.GetFunctorType(assemblyEmitter, voidType, arguments);

            var functorField = new FieldDefinition("myFunction_BackingField", FieldAttributes.Public | FieldAttributes.Static, functorType);
            typeEmitter.AddField(functorField);

            #region Setter

            var functorSetter = new MethodEmitter(typeEmitter, "set_MyFunction", voidType, MethodAttributes.Public | MethodAttributes.Static);
            var functorSetterArgument = new ParameterDefinition("value", ParameterAttributes.None, functorType);
            functorSetter.AddArgument(functorSetterArgument);

            functorSetter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = functorField,
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                IsMethodStatic = true,
                                Param = functorSetterArgument
                            }
                        }
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
                        Expression = new FieldNode()
                        {
                            Field = functorField
                        }
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
            var delegateSetterArgument = new ParameterDefinition("value", ParameterAttributes.None, delegateType);
            delegateSetter.AddArgument(delegateSetterArgument);

            delegateSetter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = delegateField,
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                IsMethodStatic = true,
                                Param = delegateSetterArgument
                            }
                        }
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
                        Expression = new FieldNode()
                        {
                            Field = delegateField
                        }
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
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new PropertyNode()
                            {
                                Property = delegateProperty
                            },
                            RightOperand = new PropertyNode()
                            {
                                Property = functorProperty
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_FunctorPropertyAssignmentToDelegate.il";
            AssertSuccessByILComparison();
        }

        private TypeReference GetFunctorType(TypeReference returnType, params TypeReference[] args)
        {
            return AssemblyRegistry.GetFunctorType(assemblyEmitter, returnType, args);
        }

        [TestMethod, TestCategory("Codegen Tests")]
        public void Test_FunctorNamesDoNotClash()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

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
