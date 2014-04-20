using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
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

        public MethodReference Get()
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

        protected void Emit(IParserNode node, bool emitReference)
        {
            switch (node.Type)
            {
                case NodeType.CodeBlockNode:
                    Emit((ICodeBlockNode)node);
                    return;

                case NodeType.ConditionBlock:
                    Emit((IConditionBlock)node);
                    return;

                case NodeType.Expression:
                    Emit((IExpressionNode)node, emitReference);
                    return;

                case NodeType.ReturnNode:
                    Emit((IReturnNode)node);
                    return;

                case NodeType.SymbolDeclaration:
                    Emit((ISymbolDeclarationNode)node);
                    return;

                case NodeType.WhileBlock:
                    Emit((IWhileBlockNode)node);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown IParserNode type: {0}", node.Type));
            }
        }

        #region Parser node

        protected void Emit(ICodeBlockNode codeBlock)
        {
            foreach (var node in codeBlock.Nodes)
            {
                Emit(node, false);
            }
        }

        protected void Emit(IConditionBlock conditionBlock)
        {
            var elseBlock = CreateLabel();
            var end = CreateLabel();

            Emit(conditionBlock.Condition, false);

            if (conditionBlock.FalseBlock != null)
            {
                Brfalse(elseBlock);
            }
            else
            {
                Brfalse(end);
            }

            Emit(conditionBlock.TrueBlock);

            if (conditionBlock.FalseBlock != null)
            {
                Br(end);
                Emit(elseBlock);
                Emit(conditionBlock.FalseBlock);
            }

            Emit(end);
        }

        protected void Emit(IExpressionNode expression, bool emitReference)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.LValue:
                    Emit((ILValueNode)expression, emitReference);
                    return;

                case ExpressionNodeType.RValue:
                    Emit((IRValueNode)expression);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown expression node type: {0}.", expression.ExpressionType));
            }
        }

        protected void Emit(IReturnNode returnNode)
        {
            Emit(returnNode.Expression, false);
            Ret();
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
                Emit(symbolDeclaration.Initializer, false);
                EmitStore(symbolDeclaration.DeclaredSymbol);
            }
        }

        protected void Emit(IWhileBlockNode whileBlockNode)
        {
            var loopStart = CreateLabel();
            var loopEnd = CreateLabel();

            Emit(loopStart);

            Emit(whileBlockNode.Condition, false);
            Brfalse(loopEnd);

            Emit(whileBlockNode.ExecutedBlock);
            Br(loopStart);

            Emit(loopEnd);
        }

        #endregion

        #region Expression node

        protected void Emit(ILValueNode lvalue, bool emitReference)
        {
            switch (lvalue.LValueType)
            {
                case LValueNodeType.Field:
                    Emit((IFieldNode)lvalue, emitReference);
                    return;

                case LValueNodeType.FunctionArgument:
                    Emit((IFunctionArgumentNode)lvalue, emitReference);
                    return;

                case LValueNodeType.LocalVariable:
                    Emit((ILocalVariableNode)lvalue, emitReference);
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

        protected void Emit(IFieldNode field, bool emitReference)
        {
            emitReference &= field.Field.FieldType.IsValueType;

            if (!field.Field.Resolve().IsStatic)
            {
                if (field.ObjectInstance != null)
                {
                    Emit(field.ObjectInstance, true);
                }
                else
                {
                    Ldarg(0);
                }

                if (emitReference)
                {
                    Ldflda(field.Field);
                }
                else
                {
                    Ldfld(field.Field);
                }
            }
            else
            {
                if (emitReference)
                {
                    Ldsflda(field.Field);
                }
                else
                {
                    Ldsfld(field.Field);
                }
            }
        }

        protected void Emit(IFunctionArgumentNode argument, bool emitReference)
        {
            var index = argument.Param.Index + (argument.IsFunctionStatic ? 0 : 1);
            emitReference &= argument.Param.ParameterType.IsValueType;

            if (emitReference)
            {
                Ldarga(index);
            }
            else
            {
                Ldarg(index);
            }
        }

        protected void Emit(ILocalVariableNode variable, bool emitReference)
        {
            emitReference &= variable.LocalVariable.VariableType.IsValueType;

            if (emitReference)
            {
                Ldloca(variable.LocalVariable.Index);
            }
            else
            {
                Ldloc(variable.LocalVariable.Index);
            }
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
                if (property.ObjectInstance != null)
                {
                    Emit(property.ObjectInstance, true);
                }
                else
                {
                    Ldarg(0);
                }
            }

            Call(getter);
        }

        #endregion

        #region Store lvalue node

        protected void EmitStore(IFieldNode field)
        {
            if (!field.Field.Resolve().IsStatic)
            {
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
            var setter = AssemblyRegistry.GetPropertySetter(Assembly, property.Property);

            if (setter == null)
            {
                throw new ArgumentException(string.Format("Property {0} has no setter.", property.Property.FullName));
            }

            Call(setter);
        }

        #endregion

        #endregion

        #region RValue node

        #region Assignment

        // There are 2 types of assignment
        //
        // LValue <- Value
        // Delegate <- Functor
        //
        // The reason Delegate <- Functor assignment is special is because
        // we actually have to call Functor.AsDelegate() to emit right side
        //
        // Duplicating in stack can happen in two ways. 
        // Asterisks show which instructions would not be present if we're not duplicating
        // If left side is a non static field:
        //  - Emit object instance
        //  - Emit right side
        //  - Dup (*)
        //  - Stfld left side
        //  - Ldfld left side (*)
        // Else if left side is a non static property:
        //  - Emit object instance
        //  - Emit right side
        //  - Dup (*)
        //  - Stloc temp (*)
        //  - Call setter
        //  - Ldloc temp (*)
        // Else:
        //  - Emit right side
        //  - Dup (*)
        //  - EmitStore left side
        // We need to store value in case of setting to a non static property because
        // we cannot guarantee whether getter will return the same result as we set in the setter
        // (that would also be inefficient even if we could)
        protected void Emit(IAssignmentOperatorNode assignmentOperator, bool duplicateValueInStack = true)
        {
            // If we're storing to field or property and it's not static, we need to load object instance now
            // and if we're also duplicating value, we got to save it to temp variable
            bool isField = assignmentOperator.LeftOperand.LValueType == LValueNodeType.Field;
            bool isProperty = assignmentOperator.LeftOperand.LValueType == LValueNodeType.Property;
            IExpressionNode objectInstance = null;
            VariableDefinition tempVariable = null;

            bool isNonStaticMember = false;

            if (isField)
            {
                var fieldNode = (IFieldNode)assignmentOperator.LeftOperand;

                if (!fieldNode.Field.Resolve().IsStatic)
                {
                    isNonStaticMember = true;
                    objectInstance = fieldNode.ObjectInstance;
                }
            }
            else if (isProperty)
            {
                var propertyNode = (IPropertyNode)assignmentOperator.LeftOperand;
                var property = propertyNode.Property.Resolve();

                if (property.SetMethod == null)
                {
                    throw new ArgumentException(string.Format("Property {0} has no setter!", property.FullName));
                }

                if (!property.SetMethod.IsStatic)
                {
                    isNonStaticMember = true;
                    objectInstance = propertyNode.ObjectInstance;
                }
            }

            if (isNonStaticMember)
            {
                if (objectInstance != null)
                {
                    Emit(objectInstance, true);
                }
                else
                {
                    Ldarg(0);
                }
            }

            EmitRightOperandForAssignment(assignmentOperator);

            if (duplicateValueInStack)
            {
                Dup();

                if (isNonStaticMember && isProperty)
                {
                    // Right operand could be functor and left could be delegate
                    // In that case, a delegate reference is on top of the stack
                    tempVariable = AcquireTempVariable(assignmentOperator.LeftOperand.ReturnType);
                    Stloc(tempVariable.Index);
                }
            }

            EmitStore(assignmentOperator.LeftOperand);

            if (duplicateValueInStack)
            {
                if (isNonStaticMember && isProperty)
                {
                    Ldloc(tempVariable.Index);
                    ReleaseTempVariable(tempVariable);
                }
                else if (isNonStaticMember)
                {
                    Emit(assignmentOperator.LeftOperand, false);
                }
            }
        }

        private void EmitRightOperandForAssignment(IAssignmentOperatorNode assignmentOperator)
        {
            bool rightIsFunction = assignmentOperator.RightOperand.ExpressionType == ExpressionNodeType.RValue &&
                ((IRValueNode)assignmentOperator.RightOperand).RValueType == RValueNodeType.Function;
            bool rightIsFunctor = assignmentOperator.RightOperand.ReturnType.IsFunctorType();
            bool canEmitRightAsReference = CanEmitAsReference(assignmentOperator.RightOperand);
            bool leftIsDelegate = assignmentOperator.LeftOperand.ReturnType.Resolve().BaseType.FullName == "System.MulticastDelegate";

            // We'll want to emit right operand in all cases
            Emit(assignmentOperator.RightOperand, leftIsDelegate && canEmitRightAsReference);

            if (leftIsDelegate)
            {
                // Sanity check
                if (rightIsFunction && rightIsFunctor)
                {
                    throw new ArgumentException("When function is assigned to delegate, its return type should be a delegate, not a functor!");
                }
                else if (rightIsFunctor)
                {
                    // Here we have a functor object on top of the stack OR its reference,
                    // depending whether we were able to load it
                    // We will just want to construct a delegate from its two fields

                    var delegateType = assignmentOperator.LeftOperand.ReturnType;
                    var functorType = assignmentOperator.RightOperand.ReturnType;

                    var objectInstanceField = AssemblyRegistry.GetField(Assembly, functorType, "objectInstance");
                    var functionPtrField = AssemblyRegistry.GetField(Assembly, functorType, "functionPtr");
                    var delegateCtor = AssemblyRegistry.GetCompatibleMethod(Assembly, delegateType, ".ctor", new List<string>()
                    {
                        "System.Object",
                        "System.IntPtr"
                    });

                    if (!canEmitRightAsReference)
                    {
                        // First, store it to a temp variable, then load its address twice
                        var tempVariable = AcquireTempVariable(functorType);

                        Stloc(tempVariable.Index);

                        Ldloca(tempVariable.Index);
                        Ldfld(objectInstanceField);

                        Ldloca(tempVariable.Index);
                        Ldfld(functionPtrField);

                        ReleaseTempVariable(tempVariable);
                    }
                    else
                    {   // If we were able to load address once, it means it's either an argument, local variable
                        // or field, which means its loading is cheap
                        Ldfld(objectInstanceField);

                        Emit(assignmentOperator.RightOperand, true);
                        Ldfld(functionPtrField);
                    }

                    Newobj(delegateCtor);
                }
            }
            else
            {
                EmitConversionIfNeeded(assignmentOperator.RightOperand.ReturnType, assignmentOperator.LeftOperand.ReturnType);
            }
        }

        #endregion

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

            Emit(binaryOperator.LeftOperand, false);

            Emit(binaryOperator.RightOperand, false);
            EmitConversionIfNeeded(binaryOperator.RightOperand.ReturnType, binaryOperator.LeftOperand.ReturnType);

            switch (binaryOperator.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.BinaryAnd:
                    And();
                    return;

                case BinaryOperatorNodeType.BinaryOr:
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

                case BinaryOperatorNodeType.Modulus:
                    EmitRemainder(binaryOperator);
                    return;

                case BinaryOperatorNodeType.Subtraction:
                    Sub();
                    return;

                case BinaryOperatorNodeType.Xor:
                    Xor();
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown binary operator node: {0}", binaryOperator.BinaryOperatorType));
            }
        }

        protected void Emit(IFunctionNode function)
        {
            var ctor = AssemblyRegistry.GetMethods(Assembly, function.ReturnType, ".ctor").Single();

            if (function.ObjectInstance != null)
            {
                Emit(function.ObjectInstance, true);
            }
            else
            {
                Ldnull();
            }

            Ldftn(function.Function);
            Newobj(ctor);
        }

        protected void Emit(IMethodCallNode functionCall)
        {
            var function = functionCall.Function;

            if (function.ExpressionType == ExpressionNodeType.RValue && ((IRValueNode)function).RValueType == RValueNodeType.Function)
            {
                var functionNode = (IFunctionNode)function;
                bool isStatic = functionNode.Function.Resolve().IsStatic;

                if (!isStatic)
                {
                    if (functionNode.ObjectInstance != null)
                    {
                        Emit(functionNode.ObjectInstance, true);
                    }
                    else
                    {
                        Ldarg(0);
                    }
                }
                else if (functionNode.ObjectInstance != null)
                {
                    throw new ArgumentException("Method is static but there is an object instance set!", "functionCall.Function.ObjectInstance");
                }

                for (int i = 0; i < functionCall.Arguments.Count; i++)
                {
                    Emit(functionCall.Arguments[i], false);

                    if (functionCall.Arguments[i].ReturnType.IsValueType && !functionNode.Function.Parameters[i].ParameterType.IsValueType)
                    {
                        Box(functionCall.Arguments[i].ReturnType);
                    }
                }

                if (functionNode.Function.Resolve().IsVirtual)
                {
                    Callvirt(functionNode.Function);
                }
                else
                {
                    Call(functionNode.Function);
                }
            }
            else
            {
                var invokeMethod = AssemblyRegistry.GetMethods(Assembly, function.ReturnType, "Invoke").Single();

                Emit(function, true);

                foreach (var argument in functionCall.Arguments)
                {
                    Emit(argument, false);
                }

                Call(invokeMethod);
            }
        }

        protected void Emit(ILiteralNode literal)
        {
            switch (literal.ReturnType.FullName)
            {
                case "System.Boolean":
                    Ldc_I4(literal.Value ? 1 : 0);
                    return;

                case "System.SByte":
                    Ldc_I4((sbyte)literal.Value);
                    return;

                case "System.Int16":
                    Ldc_I4((short)literal.Value);
                    return;

                case "System.Int32":
                    Ldc_I4((int)literal.Value);
                    return;

                case "System.Int64":
                    Ldc_I8((int)literal.Value);
                    return;

                case "System.Byte":
                    Ldc_I4((byte)literal.Value);
                    return;

                case "System.UInt16":
                    Ldc_I4((ushort)literal.Value);
                    return;

                case "System.UInt32":
                    Ldc_I4((int)literal.Value);
                    return;

                case "System.UInt64":
                    Ldc_I8((long)literal.Value);
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
            var ctor = AssemblyRegistry.GetCompatibleMethod(Assembly, objectCreation.ReturnType, ".ctor",
                objectCreation.Arguments.Select(x => x.ReturnType).ToList());

            foreach (var argument in objectCreation.Arguments)
            {
                Emit(argument, false);
            }

            Newobj(ctor);
        }

        protected void Emit(IUnaryOperatorNode unaryOperator)
        {
            if (unaryOperator.UnaryOperatorType == UnaryOperatorNodeType.VoidOperator)
            {
                EmitVoidOperator(unaryOperator);
                return;
            }

            Emit((ILValueNode)unaryOperator.Operand, false);

            switch (unaryOperator.UnaryOperatorType)
            {
                case UnaryOperatorNodeType.BinaryNot:
                    Not();
                    return;

                case UnaryOperatorNodeType.LogicalNot:
                    Ldc_I4(0);
                    Ceq();
                    return;

                case UnaryOperatorNodeType.Negation:
                    Neg();
                    return;

                case UnaryOperatorNodeType.PostDecrement:
                    Dup();
                    Ldc_I4(-1);
                    Add();
                    EmitStore((ILValueNode)unaryOperator.Operand);
                    return;

                case UnaryOperatorNodeType.PostIncrement:
                    Dup();
                    Ldc_I4(1);
                    Add();
                    EmitStore((ILValueNode)unaryOperator.Operand);
                    return;

                case UnaryOperatorNodeType.PreDecrement:
                    Ldc_I4(-1);
                    Add();
                    Dup();
                    EmitStore((ILValueNode)unaryOperator.Operand);
                    return;

                case UnaryOperatorNodeType.PreIncrement:
                    Ldc_I4(1);
                    Add();
                    Dup();
                    EmitStore((ILValueNode)unaryOperator.Operand);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown unary operator type: {0}", unaryOperator.UnaryOperatorType));
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
                EmitAddNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand, binaryOperator.ReturnType);
            }
        }

        protected void EmitAddNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            Emit(left, false);

            Emit(right, false);
            EmitConversionIfNeeded(right.ReturnType, left.ReturnType);

            Add();
        }

        protected void EmitAddString(IExpressionNode left, IExpressionNode right)
        {
            Emit(left, false);

            if (left.ReturnType.IsValueType)
            {
                Box(left.ReturnType);
            }

            Emit(right, false);

            if (right.ReturnType.IsValueType)
            {
                Box(right.ReturnType);
            }

            var concatMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, "System.String", "Concat", new List<string>()
                {
                    "System.Object",
                    "System.Object"
                });

            Call(concatMethod);
        }

        #endregion

        #region Logical And/Logical Or emitters

        protected void EmitLogicalAnd(IBinaryOperatorNode binaryOperator)
        {
            var emitFalse = ilProcessor.Create(OpCodes.Ldc_I4, 0);
            var end = CreateLabel();

            Emit(binaryOperator.LeftOperand, false);
            Brfalse(emitFalse);

            Emit(binaryOperator.RightOperand, false);
            Br(end);

            Emit(emitFalse);
            Emit(end);
        }

        protected void EmitLogicalOr(IBinaryOperatorNode binaryOperator)
        {
            var emitTrue = ilProcessor.Create(OpCodes.Ldc_I4, 1);
            var end = CreateLabel();

            Emit(binaryOperator.LeftOperand, false);
            Brtrue(emitTrue);

            Emit(binaryOperator.RightOperand, false);
            Br(end);

            Emit(emitTrue);
            Emit(end);
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
                EmitGreaterEqualThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand, binaryOperator.ReturnType);
            }
        }

        protected void EmitGreaterEqualThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, "System.String", "CompareOrdinal",
                new List<string>()
                {
                    "System.String",
                    "System.String"
                });

            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Clt();

            Ldc_I4(0);
            Ceq();
        }

        protected void EmitGreaterEqualThanNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            Emit(left, false);

            Emit(right, false);
            EmitConversionIfNeeded(right.ReturnType, left.ReturnType);

            Clt();
            Ldc_I4(0);
            Ceq();
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
                EmitGreaterThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand, binaryOperator.ReturnType);
            }
        }

        protected void EmitGreaterThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, "System.String", "CompareOrdinal",
                new List<string>()
                {
                    "System.String",
                    "System.String"
                });

            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Cgt();
        }

        protected void EmitGreaterThanNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            Emit(left, false);

            Emit(right, false);
            EmitConversionIfNeeded(right.ReturnType, left.ReturnType);

            Cgt();
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
                EmitLessEqualThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand, binaryOperator.ReturnType);
            }
        }

        protected void EmitLessEqualThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, "System.String", "CompareOrdinal",
                new List<string>()
                {
                    "System.String",
                    "System.String"
                });

            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Cgt();

            Ldc_I4(0);
            Ceq();
        }

        protected void EmitLessEqualThanNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            Emit(left, false);

            Emit(right, false);
            EmitConversionIfNeeded(right.ReturnType, left.ReturnType);

            Cgt();
            Ldc_I4(0);
            Ceq();
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
                EmitLessThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand, binaryOperator.ReturnType);
            }
        }

        protected void EmitLessThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, "System.String", "CompareOrdinal",
                new List<string>()
                {
                    "System.String",
                    "System.String"
                });

            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Clt();
        }

        protected void EmitLessThanNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            Emit(left, false);

            Emit(right, false);
            EmitConversionIfNeeded(right.ReturnType, left.ReturnType);

            Clt();
        }

        #endregion

        #endregion

        #region Division/Remainder emitters

        protected void EmitDivision(IBinaryOperatorNode binaryOperator)
        {
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
                Emit(binaryOperator.Operand, false);
                Pop();
            }
        }

        #endregion

        protected void Emit(Instruction instruction)
        {
            body.Instructions.Add(instruction);
        }

        protected void EmitConversionIfNeeded(TypeReference sourceType, TypeReference targetType)
        {
            if (sourceType.FullName == targetType.FullName)
            {
                return;
            }

            if (sourceType.IsValueType && !targetType.IsValueType)
            {
                Box(sourceType);
                return;
            }
            else if (!sourceType.IsValueType && targetType.IsValueType)
            {
                Unbox(targetType);
                Ldobj(targetType);
                return;
            }

            if (!sourceType.IsValueType && !targetType.IsValueType)
            {
                Castclass(targetType);
                return;
            }

            if (!sourceType.IsPrimitive || !sourceType.IsPrimitive)
            {
                throw new ArgumentException("Can't cast non primitive value types!");
            }

            switch (targetType.FullName)
            {
                case "System.SByte":
                    Conv_I1();
                    break;

                case "System.Int16":
                    Conv_I2();
                    break;

                case "System.Int32":
                    Conv_I4();
                    break;

                case "System.Int64":
                    Conv_I8();
                    break;

                case "System.IntPtr":
                    Conv_I();
                    break;

                case "System.Byte":
                    Conv_U1();
                    break;

                case "System.UInt16":
                    Conv_U2();
                    break;

                case "System.UInt32":
                    Conv_U4();
                    break;

                case "System.UInt64":
                    Conv_U8();
                    break;

                case "System.UIntPtr":
                    Conv_U();
                    break;

                case "System.Single":
                    if (sourceType.IsUnsignedInteger())
                    {
                        Conv_R_Un();
                    }
                    else
                    {
                        Conv_R4();
                    }
                    break;

                case "System.Double":
                    Conv_R8();
                    break;

                default:
                    throw new NotSupportedException(string.Format("Unknown primitive type: {0}", targetType.FullName));
            }
        }

        #endregion

        #region Helpers

        protected bool CanEmitAsReference(IExpressionNode node)
        {
            return node.ExpressionType == ExpressionNodeType.LValue && ((ILValueNode)node).LValueType != LValueNodeType.Property;
        }

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
            return binaryOperator.LeftOperand.ReturnType.IsUnsignedInteger() && binaryOperator.RightOperand.ReturnType.IsUnsignedInteger();
        }

        protected Instruction CreateLabel()
        {
            return Instruction.Create(OpCodes.Nop);
        }

        private class TempVariable
        {
            public VariableDefinition Variable { get; private set; }
            public bool IsTaken { get; set; }

            public TempVariable(VariableDefinition variable, bool isTaken)
            {
                Variable = variable;
                IsTaken = isTaken;
            }
        }

        List<TempVariable> temporaryVariables = new List<TempVariable>();

        protected VariableDefinition AcquireTempVariable(TypeReference type)
        {
            for (int i = 0; i < temporaryVariables.Count; i++)
            {
                if (!temporaryVariables[i].IsTaken)
                {
                    temporaryVariables[i].IsTaken = true;
                    return temporaryVariables[i].Variable;
                }
            }

            var variableName = "$Temp" + (temporaryVariables.Count + 1).ToString();
            var variable = new VariableDefinition(variableName, type);
            temporaryVariables.Add(new TempVariable(variable, true));
            body.Variables.Add(variable);
            return variable;
        }

        protected void ReleaseTempVariable(VariableDefinition variable)
        {
            for (int i = 0; i < temporaryVariables.Count; i++)
            {
                if (temporaryVariables[i].Variable == variable)
                {
                    temporaryVariables[i].IsTaken = false;
                    return;
                }
            }
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

        protected void Br(Instruction target)
        {
            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Br_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Br, target);
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

        protected void Callvirt(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Callvirt, method);
        }

        protected void Castclass(TypeReference targetType)
        {
            ilProcessor.Emit(OpCodes.Castclass, targetType);
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

        protected void Conv_I()
        {
            ilProcessor.Emit(OpCodes.Conv_I);
        }

        protected void Conv_I1()
        {
            ilProcessor.Emit(OpCodes.Conv_I1);
        }

        protected void Conv_I2()
        {
            ilProcessor.Emit(OpCodes.Conv_I2);
        }

        protected void Conv_I4()
        {
            ilProcessor.Emit(OpCodes.Conv_I4);
        }

        protected void Conv_I8()
        {
            ilProcessor.Emit(OpCodes.Conv_I8);
        }

        protected void Conv_R_Un()
        {
            ilProcessor.Emit(OpCodes.Conv_R_Un);
        }

        protected void Conv_R4()
        {
            ilProcessor.Emit(OpCodes.Conv_R4);
        }

        protected void Conv_R8()
        {
            ilProcessor.Emit(OpCodes.Conv_R8);
        }

        protected void Conv_U()
        {
            ilProcessor.Emit(OpCodes.Conv_U);
        }

        protected void Conv_U1()
        {
            ilProcessor.Emit(OpCodes.Conv_U1);
        }

        protected void Conv_U2()
        {
            ilProcessor.Emit(OpCodes.Conv_U2);
        }

        protected void Conv_U4()
        {
            ilProcessor.Emit(OpCodes.Conv_U4);
        }

        protected void Conv_U8()
        {
            ilProcessor.Emit(OpCodes.Conv_U8);
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

        protected void Ldarga(int index)
        {
            if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldarga_S, index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldarga, index);
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

        protected void Ldc_I8(long value)
        {
            ilProcessor.Emit(OpCodes.Ldc_I8, value);
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

        protected void Ldflda(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldflda, field);
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

        protected void Ldloca(int index)
        {
            if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldloca_S, (byte)index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldloca, index);
            }
        }

        protected void Ldnull()
        {
            ilProcessor.Emit(OpCodes.Ldnull);
        }

        protected void Ldobj(TypeReference targetType)
        {
            ilProcessor.Emit(OpCodes.Ldobj);
        }

        protected void Ldsfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldsfld, field);
        }

        protected void Ldsflda(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldsflda, field);
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

        protected void Neg()
        {
            ilProcessor.Emit(OpCodes.Neg);
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

        protected void Unbox(TypeReference targetType)
        {
            ilProcessor.Emit(OpCodes.Unbox, targetType);
        }

        protected void Xor()
        {
            ilProcessor.Emit(OpCodes.Xor);
        }

        #endregion
    }
}
