using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace LaborasLangCompiler.ILTools
{
    internal class MethodEmitter
    {
        private MethodDefinition methodDefinition;
        private MethodBody body;
        private ILProcessor ilProcessor;

        private AssemblyRegistry assemblyRegistry;
        private ModuleDefinition module;

        public bool Parsed { get; private set; }

        public MethodEmitter(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType, string name, TypeReference returnType, 
                                MethodAttributes methodAttributes = MethodAttributes.Private)
        {
            methodDefinition = new MethodDefinition(name, methodAttributes, returnType);
            declaringType.AddMethod(methodDefinition);

            this.assemblyRegistry = assemblyRegistry;
            module = declaringType.Module;

            body = methodDefinition.Body;
            ilProcessor = body.GetILProcessor();

            if (module == null)
            {
                throw new ArgumentException("Declaring type isn't assigned to module!");
            }
        }

        public ParameterDefinition AddArgument(TypeReference type, string name)
        {
            var parameter = new ParameterDefinition(name, ParameterAttributes.None, type);
            methodDefinition.Parameters.Add(parameter);

            return parameter;
        }

        public MethodReference GetAsReference()
        {
            return methodDefinition;
        }

        public void ParseTree(ICodeBlockNode tree)
        {
            Emit(tree);
            Parsed = true;
        }

        public void SetAsEntryPoint()
        {
            if ((methodDefinition.Attributes & MethodAttributes.Static) == 0)
            {
                throw new Exception("Entry point must be static!");
            }

            methodDefinition.DeclaringType.Module.EntryPoint = methodDefinition;
        }

        #region Emitters

        private void Emit(IParserNode node)
        {
            switch (node.Type)
            {
                case NodeType.CodeBlockNode:
                    Emit((ICodeBlockNode)node);
                    return;

                case NodeType.Expression:
                    Emit((IExpressionNode)node);
                    return;

                case NodeType.SymbolDeclaration:
                    Emit((ISymbolDeclarationNode)node);
                    return;
            }
        }

        #region Parser node

        private void Emit(ICodeBlockNode codeBlock)
        {
            foreach (var node in codeBlock.Nodes)
            {
                Emit(node);
            }
        }

        private void Emit(IExpressionNode expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.LValue:
                    Emit((ILValueNode)expression);
                    return;

                case ExpressionNodeType.RValue:
                    Emit((IRValueNode)expression);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown expression node type: {0}.", expression.ExpressionType);
            }
        }

        private void Emit(ISymbolDeclarationNode symbolDeclaration)
        {
            switch (symbolDeclaration.DeclaredSymbol.LValueType)
            {
                case LValueNodeType.LocalVariable:
                    body.Variables.Add(((ILocalVariableNode)symbolDeclaration.DeclaredSymbol).LocalVariable);
                    break;

                case LValueNodeType.FunctionArgument:
                    throw new NotSupportedException("Can't declare argument.");

                case LValueNodeType.Field:
                    throw new NotSupportedException("Can't declare field inside of a method.");

                default:
                    throw new NotSupportedException(string.Format("Unknown lvalue type: {0}.", symbolDeclaration.DeclaredSymbol.LValueType));
            }

            if (symbolDeclaration.Initializer != null)
            {
                Emit(symbolDeclaration.Initializer);
                EmitStore(symbolDeclaration.DeclaredSymbol);
            }
        }

        #endregion

        #region Expression node

        private void Emit(ILValueNode lvalue)
        {
            switch (lvalue.LValueType)
            {
                case LValueNodeType.Field:
                    Emit((IFieldNode)lvalue);
                    return;

                case LValueNodeType.FunctionArgument:
                    Emit((IFunctionArgumentNode)lvalue);
                    return;

                case LValueNodeType.LocalVariable:
                    Emit((ILocalVariableNode)lvalue);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown lvalue node type: {0}.", lvalue.LValueType));
            }
        }

        private void EmitStore(ILValueNode lvalue)
        {
            switch (lvalue.LValueType)
            {
                case LValueNodeType.Field:
                    EmitStore((IFieldNode)lvalue);
                    return;

                case LValueNodeType.FunctionArgument:
                    EmitStore((IFunctionArgumentNode)lvalue);
                    return;

                case LValueNodeType.LocalVariable:
                    EmitStore((ILocalVariableNode)lvalue);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown lvalue node type: {0}.", lvalue.LValueType));
            }
        }

        private void Emit(IRValueNode rvalue)
        {
            switch (rvalue.RValueType)
            {
                case RValueNodeType.AssignmentOperator:
                    Emit((IAssignmentOperatorNode)rvalue);
                    return;

                case RValueNodeType.BinaryOperator:
                    Emit((IBinaryOperatorNode)rvalue);
                    return;

                case RValueNodeType.Function:
                    Emit((IFunctionNode)rvalue);
                    return;

                case RValueNodeType.FunctionCall:
                    Emit((IFunctionCallNode)rvalue);
                    return;

                case RValueNodeType.Literal:
                    Emit((ILiteralNode)rvalue);
                    return;

                case RValueNodeType.MethodCall:
                    Emit((IMethodCallNode)rvalue);
                    return;

                case RValueNodeType.ObjectCreation:
                    Emit((IObjectCreationNode)rvalue);
                    return;

                case RValueNodeType.UnaryOperator:
                    Emit((IUnaryOperatorNode)rvalue);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown RValue node type: {0}.", rvalue.RValueType));
            }
        }

        #endregion

        #region LValue node

        #region Load lvalue node

        private void Emit(IFieldNode field)
        {
            Ldfld(field.Field);
        }

        private void Emit(IFunctionArgumentNode argument)
        {
            Ldarg(argument.Param.Index);
        }

        private void Emit(ILocalVariableNode variable)
        {
            Ldloc(variable.LocalVariable.Index);
        }

        #endregion

        #region Store rvalue node

        private void EmitStore(IFieldNode field)
        {
            Stfld(field.Field);
        }

        private void EmitStore(IFunctionArgumentNode argument)
        {
            Starg(argument.Param.Index);
        }

        private void EmitStore(ILocalVariableNode variable)
        {
            Stloc(variable.LocalVariable.Index);
        }

        #endregion

        #endregion

        #region RValue node

        private void Emit(IAssignmentOperatorNode assignmentOperator)
        {
            Emit(assignmentOperator.RightOperand);
            EmitStore(assignmentOperator.LeftOperand);
        }

        private void Emit(IBinaryOperatorNode binaryOperator)
        {
            switch (binaryOperator.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.Addition:
                    EmitAdd(binaryOperator);
                    return;
            }
            
            Emit(binaryOperator.LeftOperand);
            Emit(binaryOperator.RightOperand);

            switch (binaryOperator.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.BinaryAnd:
                    RequireInteger(binaryOperator.LeftOperand.ReturnType, "Binary AND requires both operands to be integers");
                    RequireInteger(binaryOperator.RightOperand.ReturnType, "Binary AND requires both operands to be integers");
                    And();
                    return;

                case BinaryOperatorNodeType.BinaryOr:
                    RequireInteger(binaryOperator.LeftOperand.ReturnType, "Binary OR requires both operands to be integers");
                    RequireInteger(binaryOperator.RightOperand.ReturnType, "Binary OR requires both operands to be integers");
                    Or();
                    return;

                case BinaryOperatorNodeType.Division:
                    EmitDivision(binaryOperator);
                    return;
                    
                case BinaryOperatorNodeType.Equals:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.GreaterEqualThan:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.GreaterThan:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.Less:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.LessThan:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.LogicalAnd:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.LogicalOr:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.Multiplication:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.NotEquals:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.Remainder:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.Subtraction:
                    throw new NotImplementedException();

                case BinaryOperatorNodeType.Xor:
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException(string.Format("Unknown binary operator node: {0}", binaryOperator.BinaryOperatorType));                    
            }
        }

        private void Emit(IFunctionNode function)
        {
            throw new NotImplementedException();
        }

        private void Emit(IFunctionCallNode functionCall)
        {
            throw new NotImplementedException();
        }

        private void Emit(ILiteralNode literal)
        {
            throw new NotImplementedException();
        }

        private void Emit(IMethodCallNode methodCall)
        {
            throw new NotImplementedException();
        }

        private void Emit(IObjectCreationNode objectCreation)
        {
            throw new NotImplementedException();
        }

        private void Emit(IUnaryOperatorNode unaryOperator)
        {
            throw new NotImplementedException();
        }

        private void EmitAdd(IBinaryOperatorNode binaryOperator)
        {
            var left = binaryOperator.LeftOperand;
            var right = binaryOperator.RightOperand;

            bool leftIsString = left.ReturnType.FullName == "System.String";
            bool rightIsString = left.ReturnType.FullName == "System.String";

            if (leftIsString || rightIsString)
            {
                Emit(left);

                if (left.ReturnType.IsValueType)
                {
                    Box(left.ReturnType);   
                }

                Emit(right);

                if (right.ReturnType.IsValueType)
                {
                    Box(right.ReturnType);
                }

                var concatMethod = (from x in assemblyRegistry.GetMethods("System.String", "Concat")
                                    where x.Parameters.Count == 2 
                                            && x.Parameters[0].ParameterType.FullName == "System.Object"
                                            && x.Parameters[1].ParameterType.FullName == "System.Object"
                                    select x).Single();

                Call(concatMethod);
            }
            else
            {
                Emit(left);
                Emit(right);
                Add();

                throw new NotImplementedException("Still need to implement implicit conversions (like int + float)");
            }
        }

        private void EmitDivision(IBinaryOperatorNode binaryOperator)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Validators

        private void RequireInteger(TypeReference type, string errorMessage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IL Instructions

        private void Add()
        {
            ilProcessor.Emit(OpCodes.Add);
        }

        private void And()
        {
            ilProcessor.Emit(OpCodes.Add);
        }

        private void Box(TypeReference type)
        {
            ilProcessor.Emit(OpCodes.Box, type);
        }

        private void Call(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Call, method);
        }

        private void Ldarg(int index)
        {
            if (index < 4)
            {
                switch (index)
                {
                    case 0:
                        ilProcessor.Emit(OpCodes.Ldarg_0);
                        return;

                    case 1:
                        ilProcessor.Emit(OpCodes.Ldarg_1);
                        return;

                    case 2:
                        ilProcessor.Emit(OpCodes.Ldarg_2);
                        return;

                    case 3:
                        ilProcessor.Emit(OpCodes.Ldarg_3);
                        return;
                }
            }
            else if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldarg_S, index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldarg, index);
            }
        }

        private void Ldfld(FieldDefinition field)
        {
            ilProcessor.Emit(OpCodes.Ldfld, field);
        }

        private void Ldloc(int index)
        {
            if (index < 4)
            {
                switch (index)
                {
                    case 0:
                        ilProcessor.Emit(OpCodes.Ldloc_0);
                        return;

                    case 1:
                        ilProcessor.Emit(OpCodes.Ldloc_1);
                        return;

                    case 2:
                        ilProcessor.Emit(OpCodes.Ldloc_2);
                        return;

                    case 3:
                        ilProcessor.Emit(OpCodes.Ldloc_3);
                        return;
                }
            }
            else if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldloc_S, index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldloc, index);
            }
        }

        private void Ldstr(string str)
        {
            ilProcessor.Emit(OpCodes.Ldstr, str);
        }

        private void Or()
        {
            ilProcessor.Emit(OpCodes.Or);
        }

        private void Pop()
        {
            ilProcessor.Emit(OpCodes.Pop);
        }

        private void Ret()
        {
            ilProcessor.Emit(OpCodes.Ret);
        }

        private void Starg(int index)
        {
            if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Starg_S, index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Starg, index);
            }
        }

        private void Stfld(FieldDefinition field)
        {
            ilProcessor.Emit(OpCodes.Stfld, field);
        }

        private void Stloc(int index)
        {
            if (index < 4)
            {
                switch (index)
                {
                    case 0:
                        ilProcessor.Emit(OpCodes.Stloc_0);
                        return;

                    case 1:
                        ilProcessor.Emit(OpCodes.Stloc_1);
                        return;

                    case 2:
                        ilProcessor.Emit(OpCodes.Stloc_2);
                        return;

                    case 3:
                        ilProcessor.Emit(OpCodes.Stloc_3);
                        return;
                }
            }
            else if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Stloc_S, index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Stloc, index);
            }
        }

        #endregion

        #region Emit Hello World

        public void EmitHelloWorld()
        {
            var console = assemblyRegistry.GetType("System.Console");

            var consoleWriteLine = module.Import(assemblyRegistry.GetMethods(console, "WriteLine")
                           .Where(x => x.Parameters.Count == 1 && x.Parameters[0].ParameterType.FullName == "System.String").Single());
            var consoleReadLine = module.Import(assemblyRegistry.GetMethods(console, "ReadLine").Where(x => x.Parameters.Count == 0).Single());

            Ldstr("Hello, world!");
            Call(consoleWriteLine);
            Call(consoleReadLine);

            Pop();
            Ret();

            Parsed = true;
        }

        #endregion
    }
}
