using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    public class ConstructorTests : ILTestBase
    {
        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_InstanceFieldInitializer()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));

            var initializer = new LiteralNode()
            {
                ExpressionReturnType = intType,
                Value = 2
            };

            var field = new FieldDefinition("testField", FieldAttributes.FamANDAssem | FieldAttributes.Family, intType);

            typeEmitter.AddField(field, initializer);

            ExpectedILFilePath = "TestCanEmit_InstanceFieldInitializer.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StaticFieldInitializer()
        {
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));

            var initializer = new LiteralNode()
            {
                ExpressionReturnType = floatType,
                Value = 2.0f
            };

            var field = new FieldDefinition("testField", FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static, floatType);

            typeEmitter.AddField(field, initializer);

            ExpectedILFilePath = "TestCanEmit_StaticFieldInitializer.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_InstancePropertyInitializer()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var initializer = new LiteralNode()
            {
                ExpressionReturnType = stringType,
                Value = "aaa"
            };

            var backingField = new FieldDefinition("testProperty_backingField", FieldAttributes.Private, stringType);

            var setter = new MethodEmitter(typeEmitter, "set_testProperty", voidType, MethodAttributes.Public);
            var value = new ParameterDefinition("value", ParameterAttributes.None, stringType);

            var setterBody = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                ObjectInstance = new ThisNode()
                                {
                                    ExpressionReturnType = backingField.DeclaringType
                                },
                                Field = backingField
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                Param = value
                            }
                        }
                    }
                }
            };

            setter.AddArgument(value);
            setter.ParseTree(setterBody);

            var property = new PropertyDefinition("testProperty", PropertyAttributes.None, stringType);
            property.SetMethod = (MethodDefinition)setter.Get();

            typeEmitter.AddField(backingField);
            typeEmitter.AddProperty(property, initializer);

            ExpectedILFilePath = "TestCanEmit_InstancePropertyInitializer.il";
            Test();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StaticPropertyInitializer()
        {
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var initializer = new LiteralNode()
            {
                ExpressionReturnType = boolType,
                Value = true
            };

            var backingField = new FieldDefinition("testProperty_backingField", FieldAttributes.Private | FieldAttributes.Static, boolType);

            var setter = new MethodEmitter(typeEmitter, "set_testProperty", voidType, MethodAttributes.Public | MethodAttributes.Static);
            var value = new ParameterDefinition("value", ParameterAttributes.None, boolType);

            var setterBody = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = backingField
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                Param = value,
                                IsMethodStatic = true
                            }
                        }
                    }
                }
            };

            setter.AddArgument(value);
            setter.ParseTree(setterBody);

            var property = new PropertyDefinition("testProperty", PropertyAttributes.None, boolType);
            property.SetMethod = (MethodDefinition)setter.Get();

            typeEmitter.AddField(backingField);
            typeEmitter.AddProperty(property, initializer);

            ExpectedILFilePath = "TestCanEmit_StaticPropertyInitializer.il";
            Test();
        }
    }
}
