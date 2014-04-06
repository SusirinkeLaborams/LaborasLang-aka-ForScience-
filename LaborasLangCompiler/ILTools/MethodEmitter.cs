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
                    throw new NotSupportedException("Unknown expression node type!");
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
                    throw new NotSupportedException("Unknown lvalue type");
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
                    throw new NotSupportedException("Unknown lvalue node type!");
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
                    throw new NotSupportedException("Unknown lvalue node type!");
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
                    throw new NotSupportedException("Unknown RValue node type!");
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
            throw new NotImplementedException();
        }

        private void Emit(IBinaryOperatorNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        private void Emit(IFunctionNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        private void Emit(IFunctionCallNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        private void Emit(ILiteralNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        private void Emit(IMethodCallNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        private void Emit(IObjectCreationNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        private void Emit(IUnaryOperatorNode assignmentOperator)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IL Instructions

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
