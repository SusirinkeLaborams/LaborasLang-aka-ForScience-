using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace LaborasLangCompiler.ILTools.Methods
{
    internal class MethodEmitter
    {
        protected MethodDefinition methodDefinition;
        protected MethodBody body;
        protected ILProcessor ilProcessor;

        protected TypeEmitter DeclaringType { get; private set; }
        protected AssemblyEmitter Assembly { get { return DeclaringType.Assembly; } }
        
        public bool Parsed { get; protected set; }

        public MethodEmitter(TypeEmitter declaringType, string name, TypeReference returnType, 
                                MethodAttributes methodAttributes = MethodAttributes.Private)
        {
            DeclaringType = declaringType;

            methodDefinition = new MethodDefinition(name, methodAttributes, returnType);
            declaringType.AddMethod(methodDefinition);

            body = methodDefinition.Body;
            ilProcessor = body.GetILProcessor();
        }

        public ParameterDefinition AddArgument(TypeReference type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Invalid argument name: " + name != null ? name : "<null>");
            }

            var parameter = new ParameterDefinition(name, ParameterAttributes.None, type);
            methodDefinition.Parameters.Add(parameter);

            return parameter;
        }

        public ParameterDefinition AddArgument(ParameterDefinition parameter)
        {
            methodDefinition.Parameters.Add(parameter);
            return parameter;
        }

        protected ParameterDefinition AddArgument(TypeReference type)
        {
            var parameter = new ParameterDefinition(type);
            methodDefinition.Parameters.Add(parameter);

            return parameter;
        }

        public MethodDefinition Get()
        {
            return methodDefinition;
        }

        public void ParseTree(ICodeBlockNode tree)
        {
            if (Parsed)
            {
                throw new InvalidOperationException("Can't parse same method twice!");
            }

            Emit(tree);
            Ret();

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

        protected void Emit(IParserNode node)
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

        protected void Emit(ICodeBlockNode codeBlock)
        {
            foreach (var node in codeBlock.Nodes)
            {
                Emit(node);
            }
        }

        protected void Emit(IExpressionNode expression)
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
                    throw new NotSupportedException(string.Format("Unknown expression node type: {0}.", expression.ExpressionType));
            }
        }

        protected void Emit(ISymbolDeclarationNode symbolDeclaration)
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

        protected void Emit(ILValueNode lvalue)
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

                case LValueNodeType.Property:
                    Emit((IPropertyNode)lvalue);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown lvalue node type: {0}.", lvalue.LValueType));
            }
        }

        protected void EmitStore(ILValueNode lvalue)
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

                case LValueNodeType.Property:
                    EmitStore((IPropertyNode)lvalue);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown lvalue node type: {0}.", lvalue.LValueType));
            }
        }

        protected void Emit(IRValueNode rvalue)
        {
            switch (rvalue.RValueType)
            {
                case RValueNodeType.AssignmentOperator:
                    Emit((IAssignmentOperatorNode)rvalue);
                    return;

                case RValueNodeType.BinaryOperator:
                    Emit((IBinaryOperatorNode)rvalue);
                    return;

                case RValueNodeType.Call:
                    Emit((IMethodCallNode)rvalue);
                    return;

                case RValueNodeType.Function:
                    Emit((IFunctionNode)rvalue);
                    return;

                case RValueNodeType.Literal:
                    Emit((ILiteralNode)rvalue);
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

        protected void Emit(IFieldNode field)
        {
            if (!field.Field.IsStatic)
            {
                Emit(field.ObjectInstance);
                Ldfld(field.Field);
            }
            else
            {
                Ldsfld(field.Field);
            }
        }

        protected void Emit(IFunctionArgumentNode argument)
        {
            Ldarg(argument.Param.Index);
        }

        protected void Emit(ILocalVariableNode variable)
        {
            Ldloc(variable.LocalVariable.Index);
        }

        protected void Emit(IPropertyNode property)
        {
            var getter = AssemblyRegistry.GetPropertyGetter(Assembly, property.Property);

            if (getter == null)
            {
                throw new ArgumentException(string.Format("Property {0} has no getter.", property.Property.FullName));
            }

            if (!getter.Resolve().IsStatic)
            {
                Emit(property.ObjectInstance);
            }

            Call(getter);
        }

        #endregion

        #region Store lvalue node

        protected void EmitStore(IFieldNode field)
        {
            if (!field.Field.IsStatic)
            {
                Emit(field.ObjectInstance);
                Stfld(field.Field);
            }
            else
            {
                Stsfld(field.Field);
            }
        }

        protected void EmitStore(IFunctionArgumentNode argument)
        {
            Starg(argument.Param.Index);
        }

        protected void EmitStore(ILocalVariableNode variable)
        {
            Stloc(variable.LocalVariable.Index);
        }

        protected void EmitStore(IPropertyNode property)
        {
            var setter = property.Property.SetMethod;

            if (setter == null)
            {
                throw new ArgumentException(string.Format("Property {0} has no setter.", property.Property.FullName));
            }

            if (!setter.IsStatic)
            {
                Emit(property.ObjectInstance);
            }

            Call(setter);
        }

        #endregion

        #endregion

        #region RValue node

        protected void Emit(IAssignmentOperatorNode assignmentOperator, bool duplicateValueInStack = true)
        {
            Emit(assignmentOperator.RightOperand);

            if (duplicateValueInStack)
            {
                Dup();
            }

            EmitStore(assignmentOperator.LeftOperand);
        }

        protected void Emit(IBinaryOperatorNode binaryOperator)
        {
            switch (binaryOperator.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.Addition:
                    EmitAdd(binaryOperator);
                    return;

                case BinaryOperatorNodeType.LogicalAnd:
                    EmitLogicalAnd(binaryOperator);
                    return;

                case BinaryOperatorNodeType.LogicalOr:
                    EmitLogicalOr(binaryOperator);
                    return;

                case BinaryOperatorNodeType.GreaterEqualThan:
                    EmitGreaterEqualThan(binaryOperator);
                    return;

                case BinaryOperatorNodeType.GreaterThan:
                    EmitGreaterThan(binaryOperator);
                    return;

                case BinaryOperatorNodeType.LessEqualThan:
                    EmitLessEqualThan(binaryOperator);
                    return;

                case BinaryOperatorNodeType.LessThan:
                    EmitLessThan(binaryOperator);
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
                    Ceq();
                    return;
                    
                case BinaryOperatorNodeType.Multiplication:
                    Mul();
                    return;

                case BinaryOperatorNodeType.NotEquals:
                    Ceq();
                    Ldc_I4(0);
                    Ceq();
                    return;

                case BinaryOperatorNodeType.Remainder:
                    EmitRemainder(binaryOperator);
                    return;

                case BinaryOperatorNodeType.Subtraction:
                    Sub();
                    return;

                case BinaryOperatorNodeType.Xor:
                    RequireInteger(binaryOperator.LeftOperand.ReturnType, "XOR requires both operands to be integers");
                    RequireInteger(binaryOperator.RightOperand.ReturnType, "XOR requires both operands to be integers");
                    Xor();
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown binary operator node: {0}", binaryOperator.BinaryOperatorType));                    
            }
        }
        
        protected void Emit(IFunctionNode function)
        {
            Ldftn(function.Function);
        }

        protected void Emit(IMethodCallNode functionCall)
        {
            var function = functionCall.Function;
            
            if (function.ExpressionType == ExpressionNodeType.RValue && ((IRValueNode)function).RValueType == RValueNodeType.Function)
            {
                var functionNode = (IFunctionNode)function;
                bool isStatic = functionNode.Function.Resolve().IsStatic;

                if (functionNode.ObjectInstance != null)
                {
                    if (!isStatic)
                    {
                        Emit(functionNode.ObjectInstance);
                    }
                    else
                    {
                        throw new ArgumentException("Method is static but there is an object instance set!", "functionCall.Function.ObjectInstance");
                    }
                }
                else if (!isStatic)
                {
                    throw new ArgumentNullException("Method is not static but there is no object instance set!", "functionCall.Function.ObjectInstance");
                }

                foreach (var argument in functionCall.Arguments)
                {
                    Emit(argument);
                }

                Call(functionNode.Function);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected void Emit(ILiteralNode literal)
        {
            switch (literal.ReturnType.FullName)
            {
                case "System.Boolean":
                    Ldc_I4(literal.Value ? 1 : 0);
                    return;

                case "System.Int32":
                    Ldc_I4((int)literal.Value);
                    return;

                case "System.Single":
                    Ldc_R4((float)literal.Value);
                    return;

                case "System.Double":
                    Ldc_R8((double)literal.Value);
                    return;
                    
                case "System.String":
                    Ldstr((string)literal.Value);
                    return;

                default:
                    throw new NotSupportedException("Unknown literal type: " + literal.ReturnType.FullName);
            }
        }

        protected void Emit(IObjectCreationNode objectCreation)
        {
            throw new NotImplementedException();
        }

        protected void Emit(IUnaryOperatorNode unaryOperator)
        {
            switch (unaryOperator.UnaryOperatorType)
            {
                case UnaryOperatorNodeType.BinaryNot:
                    RequireInteger(unaryOperator.Operand.ReturnType, "Binary negation requires integer operand.");
                    Emit(unaryOperator.Operand);
                    Not();
                    return;

                case UnaryOperatorNodeType.LogicalNot:
                    RequireBoolean(unaryOperator.Operand.ReturnType, "Logical not requires boolean operand.");
                    throw new NotImplementedException();

                case UnaryOperatorNodeType.Negation:
                    RequireNumeral(unaryOperator.Operand.ReturnType, "Negation requires numeral operand.");
                    throw new NotImplementedException();

                case UnaryOperatorNodeType.PostDecrement:
                    throw new NotImplementedException();

                case UnaryOperatorNodeType.PostIncrement:
                    throw new NotImplementedException();

                case UnaryOperatorNodeType.PreDecrement:
                    throw new NotImplementedException();

                case UnaryOperatorNodeType.PreIncrement:
                    throw new NotImplementedException();

                case UnaryOperatorNodeType.VoidOperator:
                    EmitVoidOperator(unaryOperator);
                    return;
            }
        }

        #region Add emitter

        protected void EmitAdd(IBinaryOperatorNode binaryOperator)
        {
            if (IsAtLeastOneOperandString(binaryOperator))
            {
                EmitAddString(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
            else
            {
                EmitAddNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
        }

        protected void EmitAddNumeral(IExpressionNode left, IExpressionNode right)
        {
            RequireNumeral(left.ReturnType, "Addition requires both operands to be numerals or at least one to be a string");
            RequireNumeral(right.ReturnType, "Addition requires both operands to be numerals or at least one to be a string");

            Emit(left);
            Emit(right);
            Add();

            throw new NotImplementedException("Still need to implement implicit conversions (like int + float)");
        }

        protected void EmitAddString(IExpressionNode left, IExpressionNode right)
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

            var concatMethod = (from x in AssemblyRegistry.GetMethods(Assembly, "System.String", "Concat")
                                where x.Parameters.Count == 2
                                        && x.Parameters[0].ParameterType.FullName == "System.Object"
                                        && x.Parameters[1].ParameterType.FullName == "System.Object"
                                select x).Single();

            Call(concatMethod);
        }

        #endregion

        #region Logical And/Logical Or emitters

        protected void EmitLogicalAnd(IBinaryOperatorNode binaryOperator)
        {
            RequireBoolean(binaryOperator.LeftOperand.ReturnType, "Logical AND requires both operands to be booleans");
            RequireBoolean(binaryOperator.RightOperand.ReturnType, "Logical AND requires both operands to be booleans");

            throw new NotImplementedException();
        }

        protected void EmitLogicalOr(IBinaryOperatorNode binaryOperator)
        {
            RequireBoolean(binaryOperator.LeftOperand.ReturnType, "Logical OR requires both operands to be booleans");
            RequireBoolean(binaryOperator.RightOperand.ReturnType, "Logical OR requires both operands to be booleans");

            throw new NotImplementedException();
        }

        #endregion

        #region Comparison emitters

        #region Greater equal than emitter

        protected void EmitGreaterEqualThan(IBinaryOperatorNode binaryOperator)
        {
            if (IsAtLeastOneOperandString(binaryOperator))
            {
                EmitGreaterEqualThanString(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
            else
            {
                EmitGreaterEqualThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
        }

        protected void EmitGreaterEqualThanString(IExpressionNode left, IExpressionNode right)
        {
            throw new NotImplementedException();
        }

        protected void EmitGreaterEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            RequireNumeral(left.ReturnType, "Greater equal than requires both operands to be numerals or at least one to be a string");
            RequireNumeral(right.ReturnType, "Greater equal than requires both operands to be numerals or at least one to be a string");

            Emit(left);
            Emit(right);

            Clt();
            Ldc_I4(0);
            Ceq();

            throw new NotImplementedException();    // Conversions int <---> float, etc
        }

        #endregion

        #region Greater than emitter

        protected void EmitGreaterThan(IBinaryOperatorNode binaryOperator)
        {
            if (IsAtLeastOneOperandString(binaryOperator))
            {
                EmitGreaterThanString(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
            else
            {
                EmitGreaterThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
        }

        protected void EmitGreaterThanString(IExpressionNode left, IExpressionNode right)
        {
            throw new NotImplementedException();
        }

        protected void EmitGreaterThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            RequireNumeral(left.ReturnType, "Greater than requires both operands to be numerals or at least one to be a string");
            RequireNumeral(right.ReturnType, "Greater than requires both operands to be numerals or at least one to be a string");

            Emit(left);
            Emit(right);

            Cgt();

            throw new NotImplementedException();    // Conversions int <---> float, etc
        }

        #endregion

        #region Less equal than emitter

        protected void EmitLessEqualThan(IBinaryOperatorNode binaryOperator)
        {
            if (IsAtLeastOneOperandString(binaryOperator))
            {
                EmitLessEqualThanString(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
            else
            {
                EmitLessEqualThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
        }

        protected void EmitLessEqualThanString(IExpressionNode left, IExpressionNode right)
        {
            throw new NotImplementedException();
        }

        protected void EmitLessEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            RequireNumeral(left.ReturnType, "Less equal than requires both operands to be numerals or at least one to be a string");
            RequireNumeral(right.ReturnType, "Less equal than requires both operands to be numerals or at least one to be a string");

            Emit(left);
            Emit(right);

            Cgt();
            Ldc_I4(0);
            Ceq();

            throw new NotImplementedException();    // Conversions int <---> float, etc
        }

        #endregion

        #region Emit less than

        protected void EmitLessThan(IBinaryOperatorNode binaryOperator)
        {
            if (IsAtLeastOneOperandString(binaryOperator))
            {
                EmitLessThanString(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
            else
            {
                EmitLessThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
            }
        }

        protected void EmitLessThanString(IExpressionNode left, IExpressionNode right)
        {
            throw new NotImplementedException();
        }

        protected void EmitLessThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            RequireNumeral(left.ReturnType, "Less than requires both operands to be numerals or at least one to be a string");
            RequireNumeral(right.ReturnType, "Less than requires both operands to be numerals or at least one to be a string");

            Emit(left);
            Emit(right);

            Clt();

            throw new NotImplementedException();    // Conversions int <---> float, etc
        }

        #endregion

        #endregion

        #region Division/Remainder emitters

        protected void EmitDivision(IBinaryOperatorNode binaryOperator)
        {
            RequireNumeral(binaryOperator.LeftOperand.ReturnType, "Division requires both operands to be numerals");
            RequireNumeral(binaryOperator.RightOperand.ReturnType, "Division requires both operands to be numerals");

            if (AreBothOperandsUnsigned(binaryOperator))
            {
                Div_Un();
            }
            else
            {
                Div();
            }
        }

        protected void EmitRemainder(IBinaryOperatorNode binaryOperator)
        {
            RequireNumeral(binaryOperator.LeftOperand.ReturnType, "Remainder requires both operands to be numerals");
            RequireNumeral(binaryOperator.RightOperand.ReturnType, "Remainder requires both operands to be numerals");

            if (AreBothOperandsUnsigned(binaryOperator))
            {
                Rem_Un();
            }
            else
            {
                Rem();
            }
        }

        #endregion

        protected void EmitVoidOperator(IUnaryOperatorNode binaryOperator)
        {
            if (binaryOperator.Operand.ExpressionType == ExpressionNodeType.RValue &&
                ((IRValueNode)binaryOperator.Operand).RValueType == RValueNodeType.AssignmentOperator)
            {
                Emit(((IAssignmentOperatorNode)binaryOperator.Operand), false);
            }
            else
            {
                Emit(binaryOperator.Operand);
                Pop();
            }
        }

        #endregion

        protected void Emit(Instruction instruction)
        {
            body.Instructions.Add(instruction);
        }

        #endregion

        #region Validators

        private void RequireNumeral(TypeReference type, string errorMessage)
        {
            throw new NotImplementedException();
        }

        private void RequireInteger(TypeReference type, string errorMessage)
        {
            throw new NotImplementedException();
        }

        private void RequireBoolean(TypeReference type, string errorMessage)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helpers


        protected bool IsAtLeastOneOperandString(IBinaryOperatorNode binaryOperator)
        {
            var left = binaryOperator.LeftOperand;
            var right = binaryOperator.RightOperand;

            bool leftIsString = left.ReturnType.FullName == "System.String";
            bool rightIsString = left.ReturnType.FullName == "System.String";

            return leftIsString || rightIsString;
        }

        protected bool AreBothOperandsUnsigned(IBinaryOperatorNode binaryOperator)
        {
            throw new NotImplementedException();
        }

        protected Instruction CreateLabel()
        {
            return Instruction.Create(OpCodes.Nop);
        }

        #endregion

        #region IL Instructions

        protected void Add()
        {
            ilProcessor.Emit(OpCodes.Add);
        }

        protected void And()
        {
            ilProcessor.Emit(OpCodes.Add);
        }

        protected void Box(TypeReference type)
        {
            ilProcessor.Emit(OpCodes.Box, type);
        }

        protected void Brfalse(Instruction target)
        {
            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Brfalse_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Brfalse, target);
            }
        }

        protected void Brtrue(Instruction target)
        {
            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Brtrue_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Brtrue, target);
            }
        }

        protected void Call(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Call, method);
        }

        protected void Calli(CallSite callSite)
        {
            ilProcessor.Emit(OpCodes.Calli, callSite);
        }

        protected void Ceq()
        {
            ilProcessor.Emit(OpCodes.Ceq);
        }

        protected void Cgt()
        {
            ilProcessor.Emit(OpCodes.Cgt);
        }

        protected void Clt()
        {
            ilProcessor.Emit(OpCodes.Clt);
        }

        protected void Div()
        {
            ilProcessor.Emit(OpCodes.Div);
        }

        protected void Div_Un()
        {
            ilProcessor.Emit(OpCodes.Div_Un);
        }

        protected void Dup()
        {
            ilProcessor.Emit(OpCodes.Dup);
        }

        protected void Ldarg(int index)
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

        protected void Ldc_I4(int value)
        {
            if (value > -2 && value < 9)
            {
                switch (value)
                {
                    case -1:
                        ilProcessor.Emit(OpCodes.Ldc_I4_M1);
                        return;

                    case 0:
                        ilProcessor.Emit(OpCodes.Ldc_I4_0);
                        return;

                    case 1:
                        ilProcessor.Emit(OpCodes.Ldc_I4_1);
                        return;

                    case 2:
                        ilProcessor.Emit(OpCodes.Ldc_I4_2);
                        return;

                    case 3:
                        ilProcessor.Emit(OpCodes.Ldc_I4_3);
                        return;

                    case 4:
                        ilProcessor.Emit(OpCodes.Ldc_I4_4);
                        return;

                    case 5:
                        ilProcessor.Emit(OpCodes.Ldc_I4_5);
                        return;

                    case 6:
                        ilProcessor.Emit(OpCodes.Ldc_I4_6);
                        return;

                    case 7:
                        ilProcessor.Emit(OpCodes.Ldc_I4_7);
                        return;

                    case 8:
                        ilProcessor.Emit(OpCodes.Ldc_I4_8);
                        return;
                }
            }
            else if (value > -129 && value < 128)
            {
                ilProcessor.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldc_I4, value);
            }
        }

        protected void Ldc_R4(float value)
        {
            ilProcessor.Emit(OpCodes.Ldc_R4, value);
        }

        protected void Ldc_R8(double value)
        {
            ilProcessor.Emit(OpCodes.Ldc_R8, value);
        }

        protected void Ldfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldfld, field);
        }

        protected void Ldftn(MethodReference function)
        {
            ilProcessor.Emit(OpCodes.Ldftn, function);
        }
        
        protected void Ldloc(int index)
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

        protected void Ldsfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldsfld, field);
        }

        protected void Ldstr(string str)
        {
            ilProcessor.Emit(OpCodes.Ldstr, str);
        }
        
        protected void Mul()
        {
            ilProcessor.Emit(OpCodes.Mul);
        }

        protected void Newobj(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Newobj, method);
        }

        protected void Or()
        {
            ilProcessor.Emit(OpCodes.Or);
        }

        protected void Nop()
        {
            ilProcessor.Emit(OpCodes.Nop);
        }

        protected void Not()
        {
            ilProcessor.Emit(OpCodes.Not);
        }

        protected void Pop()
        {
            ilProcessor.Emit(OpCodes.Pop);
        }

        protected void Rem()
        {
            ilProcessor.Emit(OpCodes.Rem);
        }

        protected void Rem_Un()
        {
            ilProcessor.Emit(OpCodes.Rem_Un);
        }

        protected void Ret()
        {
            ilProcessor.Emit(OpCodes.Ret);
        }

        protected void Starg(int index)
        {
            if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Starg_S, (byte)index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Starg, index);
            }
        }

        protected void Stfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Stfld, field);
        }

        protected void Stloc(int index)
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

        protected void Stsfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Stsfld, field);
        }

        protected void Sub()
        {
            ilProcessor.Emit(OpCodes.Sub);
        }

        protected void Xor()
        {
            ilProcessor.Emit(OpCodes.Xor);
        }

        #endregion
    }
}
