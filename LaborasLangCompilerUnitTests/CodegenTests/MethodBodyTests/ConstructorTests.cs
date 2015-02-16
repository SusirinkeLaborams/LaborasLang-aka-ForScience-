using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
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
    public class ConstructorTests : CodegenTestBase
    {
        public ConstructorTests()
        {
        }

        internal ConstructorTests(AssemblyEmitter assemblyEmitter, string className, bool bulkTesting) :
            base(assemblyEmitter, className, bulkTesting)
        {
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_InstanceFieldInitializer()
        {
            var intType = assemblyEmitter.TypeSystem.Int32;

            var initializer = new LiteralNode(intType, 2);
            var field = new FieldDefinition("testField", FieldAttributes.FamANDAssem | FieldAttributes.Family, intType);

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, initializer);

            var loadFieldExpression = new FieldNode()
            {
                ObjectInstance = new ObjectCreationNode()
                {
                    ExpressionReturnType = typeEmitter.Get(assemblyEmitter),
                    Constructor = AssemblyRegistry.GetMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter), ".ctor"),
                    Args = new List<IExpressionNode>()
                },
                Field = field
            };

            GenerateBodyToOutputExpression(loadFieldExpression);
            ExpectedOutput = "2";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StaticFieldInitializer()
        {
            var floatType = assemblyEmitter.TypeSystem.Single;

            var initializer = new LiteralNode(floatType, 2.0f);
            var field = new FieldDefinition("testField", FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static, floatType);

            typeEmitter.AddField(field);
            typeEmitter.AddFieldInitializer(field, initializer);
            
            GenerateBodyToOutputExpression(new FieldNode(field));
            ExpectedOutput = 2.0f.ToString();
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_InstancePropertyInitializer()
        {
            var stringType = assemblyEmitter.TypeSystem.String;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var initializer = new LiteralNode(stringType, "aaa");
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
                            RightOperand = new ParameterNode(value)
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

            var loadFieldExpression = new FieldNode()
            {
                ObjectInstance = new ObjectCreationNode()
                {
                    ExpressionReturnType = typeEmitter.Get(assemblyEmitter),
                    Constructor = AssemblyRegistry.GetMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter), ".ctor"),
                    Args = new List<IExpressionNode>()
                },
                Field = backingField
            };

            GenerateBodyToOutputExpression(loadFieldExpression);
            ExpectedOutput = "aaa";
            AssertSuccessByExecution();
        }

        [TestMethod, TestCategory("Execution Based Codegen Tests")]
        public void TestCanEmit_StaticPropertyInitializer()
        {
            var boolType = assemblyEmitter.TypeSystem.Boolean;
            var voidType = assemblyEmitter.TypeSystem.Void;

            var initializer = new LiteralNode(boolType, true);
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
                            LeftOperand = new FieldNode(backingField),
                            RightOperand = new ParameterNode(value)
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

            GenerateBodyToOutputExpression(new FieldNode(backingField));
            ExpectedOutput = true.ToString();
            AssertSuccessByExecution();
        }
    }
}
