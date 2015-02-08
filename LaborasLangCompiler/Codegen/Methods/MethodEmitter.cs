using LaborasLangCompiler.Codegen.MethodBodyOptimizers;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaborasLangCompiler.Codegen.Methods
{
    internal class MethodEmitter : MethodEmitterBase
    {
        public bool Parsed { get; protected set; }

        public MethodEmitter(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes = MethodAttributes.Private) :
            base(declaringType, name, returnType, methodAttributes)
        {
        }
        
        public void ParseTree(ICodeBlockNode tree)
        {
            if (Parsed)
            {
                throw new InvalidOperationException("Can't parse same method twice!");
            }

            Emit(tree);

            if (body.Instructions.Count == 0 || body.Instructions.Last().OpCode != OpCodes.Ret)
            {
                Ret();
            }

            if (body.Variables.Count > 0)
            {
                body.InitLocals = true;
            }

            MethodBodyOptimizerBootstraper.Optimize(body, Assembly.DebugBuild);
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

        public bool IsEntryPoint()
        {
            return methodDefinition.DeclaringType.Module.EntryPoint == methodDefinition;
        }

        #region Emitters

        protected void Emit(IParserNode node, bool emitReference)
        {
            var oldSequencePoint = CurrentSequencePoint;
            
            if (node.SequencePoint != null)
            {
                CurrentSequencePoint = node.SequencePoint;
            }

            switch (node.Type)
            {
                case NodeType.CodeBlockNode:
                    Emit((ICodeBlockNode)node);
                    break;

                case NodeType.ConditionBlock:
                    Emit((IConditionBlock)node);
                    break;

                case NodeType.Expression:
                    Emit((IExpressionNode)node, emitReference);
                    break;

                case NodeType.ReturnNode:
                    Emit((IReturnNode)node);
                    break;

                case NodeType.SymbolDeclaration:
                    Emit((ISymbolDeclarationNode)node);
                    break;

                case NodeType.WhileBlock:
                    Emit((IWhileBlockNode)node);
                    break;

                default:
                    throw new NotSupportedException(string.Format("Unknown IParserNode type: {0}", node.Type));
            }

            CurrentSequencePoint = oldSequencePoint;
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
                case ExpressionNodeType.Field:
                    Emit((IFieldNode)expression, emitReference);
                    return;

                case ExpressionNodeType.FunctionArgument:
                    Emit((IMethodParamNode)expression, emitReference);
                    return;

                case ExpressionNodeType.LocalVariable:
                    Emit((ILocalVariableNode)expression, emitReference);
                    return;

                case ExpressionNodeType.Property:
                    Emit((IPropertyNode)expression);
                    return;

                case ExpressionNodeType.AssignmentOperator:
                    Emit((IAssignmentOperatorNode)expression);
                    return;

                case ExpressionNodeType.BinaryOperator:
                    Emit((IBinaryOperatorNode)expression);
                    return;

                case ExpressionNodeType.Call:
                    Emit((IFunctionCallNode)expression);
                    return;

                case ExpressionNodeType.Function:
                    Emit((IMethodNode)expression);
                    return;

                case ExpressionNodeType.Literal:
                    Emit((ILiteralNode)expression);
                    return;

                case ExpressionNodeType.ObjectCreation:
                    Emit((IObjectCreationNode)expression);
                    return;

                case ExpressionNodeType.This:
                    EmitThis();
                    return;

                case ExpressionNodeType.UnaryOperator:
                    Emit((IUnaryOperatorNode)expression);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown expression node type: {0}.", expression.ExpressionType));
            }
        }

        protected void Emit(IReturnNode returnNode)
        {
            if (returnNode.Expression != null)
            {
                Emit(returnNode.Expression, false);
            }

            Ret();
        }

        protected void Emit(ISymbolDeclarationNode symbolDeclaration)
        {
            body.Variables.Add(symbolDeclaration.Variable);

            if (symbolDeclaration.Initializer != null)
            {
                EmitExpressionWithTargetType(symbolDeclaration.Initializer, symbolDeclaration.Variable.VariableType);
                Stloc(symbolDeclaration.Variable.Index);
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


        protected void EmitStore(IExpressionNode expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.Field:
                    EmitStore((IFieldNode)expression);
                    return;

                case ExpressionNodeType.FunctionArgument:
                    EmitStore((IMethodParamNode)expression);
                    return;

                case ExpressionNodeType.LocalVariable:
                    EmitStore((ILocalVariableNode)expression);
                    return;

                case ExpressionNodeType.Property:
                    EmitStore((IPropertyNode)expression);
                    return;

                default:
                    throw new NotSupportedException(string.Format("Cannot store {0} ExpressionNode.", expression.ExpressionType));
            }
        }

        #region Load expression node

        protected void Emit(IFieldNode field, bool emitReference)
        {
            emitReference &= field.Field.FieldType.IsValueType;

            if (!field.Field.Resolve().IsStatic)
            {
                Emit(field.ObjectInstance, true);

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

        protected void Emit(IMethodParamNode argument, bool emitReference)
        {
            var index = argument.Param.Index + (argument.IsMethodStatic ? 0 : 1);
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

            if (getter.HasThis)
            {
                Emit(property.ObjectInstance, true);
            }

            Call(getter);
        }

        #endregion

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
            bool isField = assignmentOperator.LeftOperand.ExpressionType == ExpressionNodeType.Field;
            bool isProperty = assignmentOperator.LeftOperand.ExpressionType == ExpressionNodeType.Property;
            IExpressionNode objectInstance = null;
            VariableDefinition tempVariable = null;

            bool memberHasThis = false;

            if (isField)
            {
                var fieldNode = (IFieldNode)assignmentOperator.LeftOperand;

                if (!fieldNode.Field.Resolve().IsStatic)
                {
                    memberHasThis = true;
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

                if (property.SetMethod.HasThis)
                {
                    memberHasThis = true;
                    objectInstance = propertyNode.ObjectInstance;
                }
            }

            if (memberHasThis)
            {
                Emit(objectInstance, true);
            }

            EmitExpressionWithTargetType(assignmentOperator.RightOperand, assignmentOperator.LeftOperand.ExpressionReturnType);

            if (duplicateValueInStack)
            {
                if (!memberHasThis)
                {
                    Dup();
                }
                else if (isProperty)    // HasThis and IsProperty
                {
                    // Right operand could be a different type, 
                    // so it will get casted to left operand type
                    tempVariable = AcquireTempVariable(assignmentOperator.LeftOperand.ExpressionReturnType);

                    Dup();
                    Stloc(tempVariable.Index);
                }
            }

            EmitStore(assignmentOperator.LeftOperand);

            if (duplicateValueInStack && memberHasThis)
            {
                if (isProperty)
                {
                    Ldloc(tempVariable.Index);
                    ReleaseTempVariable(tempVariable);
                }
                else
                {
                    Emit(assignmentOperator.LeftOperand, false);
                }
            }
        }

        private void EmitExpressionWithTargetType(IExpressionNode expression, TypeReference targetType, bool emitAsReference = false)
        {
            bool expressionIsFunction = expression.ExpressionType == ExpressionNodeType.Function;
            bool expressionIsFunctor = expression.ExpressionReturnType.IsFunctorType();
            bool canEmitExpressionAsReference = CanEmitAsReference(expression);

            var targetBaseType = targetType.Resolve().BaseType;
            bool targetIsDelegate = targetBaseType != null && targetBaseType.FullName == "System.MulticastDelegate";

            if (emitAsReference && !canEmitExpressionAsReference)
            {
                throw new Exception(string.Format("{0}({1},{2},{3},{4}): error: can't pass RValue by reference", expression.SequencePoint.Document.Url,
                    expression.SequencePoint.StartLine, expression.SequencePoint.StartColumn, expression.SequencePoint.EndLine, expression.SequencePoint.EndColumn));
            }

            // We'll want to emit expression in all cases
            Emit(expression, emitAsReference && canEmitExpressionAsReference);

            if (targetIsDelegate)
            {
                // Sanity check
                if (expressionIsFunction && expressionIsFunctor)
                {
                    throw new ArgumentException("When function is assigned to delegate, its return type should be a delegate, not a functor!");
                }
                else if (expressionIsFunctor)
                {
                    // Here we have a functor object on top of the stack
                    
                    var delegateType = targetType;
                    var functorType = expression.ExpressionReturnType;

                    var asDelegateMethod = AssemblyRegistry.GetMethod(Assembly, functorType, "AsDelegate");
                    var delegateInvokeMethod = AssemblyRegistry.GetMethod(Assembly, asDelegateMethod.ReturnType, "Invoke");
                    var delegateCtor = AssemblyRegistry.GetMethod(Assembly, delegateType, ".ctor");

                    Callvirt(asDelegateMethod);
                    Ldftn(delegateInvokeMethod);
                    Newobj(delegateCtor);
                }
            }
            else
            {
                EmitConversionIfNeeded(expression.ExpressionReturnType, targetType);
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

                case BinaryOperatorNodeType.ShiftLeft:
                case BinaryOperatorNodeType.ShiftRight:
                    EmitShift(binaryOperator);
                    return;
            }

            EmitOperandsAndConvertIfNeeded(binaryOperator.LeftOperand, binaryOperator.RightOperand);

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

                case BinaryOperatorNodeType.BinaryXor:
                    Xor();
                    return;

                default:
                    throw new NotSupportedException(string.Format("Unknown binary operator node: {0}", binaryOperator.BinaryOperatorType));
            }
        }

        protected void Emit(IMethodNode function)
        {
            var returnTypeIsDelegate = function.ExpressionReturnType.Resolve().BaseType.FullName == "System.MulticastDelegate";

            if (!returnTypeIsDelegate)
            {
                var functorType = AssemblyRegistry.GetImplementationFunctorType(Assembly, DeclaringType, function.Method);
                var ctor = AssemblyRegistry.GetMethod(Assembly, functorType, ".ctor");

                if (function.Method.HasThis)
                {
                    Emit(function.ObjectInstance, false);
                }

                Newobj(ctor);
            }
            else
            {
                var ctor = AssemblyRegistry.GetMethod(Assembly, function.ExpressionReturnType, ".ctor");

                if (function.Method.HasThis)
                {
                    Emit(function.ObjectInstance, false);
                }
                else
                {
                    Ldnull();
                }

                Ldftn(function.Method);
                Newobj(ctor);
            }
        }

        protected void Emit(IFunctionCallNode functionCall)
        {
            var function = functionCall.Function;

            if (function.ExpressionType == ExpressionNodeType.Function)
            {   // Direct call
                var functionNode = (IMethodNode)function;

                if (functionNode.Method.HasThis)
                {
                    Emit(functionNode.ObjectInstance, true);
                }

                EmitArgumentsForCall(functionCall.Args, functionNode.Method);

                if (functionNode.Method.Resolve().IsVirtual)
                {
                    Callvirt(functionNode.Method);
                }
                else
                {
                    Call(functionNode.Method);
                }
            }
            else
            {   // Functor Call
                var invokeMethod = AssemblyRegistry.GetMethod(Assembly, function.ExpressionReturnType, "Invoke");

                Emit(function, true);
                EmitArgumentsForCall(functionCall.Args, invokeMethod);

                Callvirt(invokeMethod);
            }
        }

        private void EmitArgumentsForCall(IReadOnlyList<IExpressionNode> arguments, MethodReference method)
        {
            var methodParameters = method.Parameters;
            var resolvedMethod = method.Resolve();

            if (method.IsParamsMethod())
            {
                #region Params Call

                for (int i = 0; i < methodParameters.Count - 1; i++)
                {
                    EmitArgumentForCall(arguments[i], methodParameters[i].ParameterType);
                }

                var arrayVariable = AcquireTempVariable(methodParameters.Last().ParameterType);
                var arrayType = methodParameters.Last().ParameterType.GetElementType();

                Ldc_I4(arguments.Count - methodParameters.Count + 1);
                Newarr(arrayType);
                Stloc(arrayVariable.Index);

                for (int i = methodParameters.Count - 1; i < arguments.Count; i++)
                {
                    Ldloc(arrayVariable.Index);
                    Ldc_I4(i - methodParameters.Count + 1);
                    EmitArgumentForCall(arguments[i], arrayType);

                    if (arrayType.IsValueType)
                    {
                        Stelem_Any(arrayType);
                    }
                    else
                    {
                        Stelem_Ref();
                    }
                }

                Ldloc(arrayVariable.Index);
                ReleaseTempVariable(arrayVariable);

                #endregion
            }
            else if (methodParameters.Count > arguments.Count)    // Method with optional parameters call
            {
                #region Optional Parameters Call

                for (int i = 0; i < arguments.Count; i++)
                {
                    EmitArgumentForCall(arguments[i], methodParameters[i].ParameterType);
                }

                for (int i = arguments.Count; i < methodParameters.Count; i++)
                {
                    var defaultValue = resolvedMethod.Parameters[i].Constant;

                    switch (defaultValue.GetType().FullName)
                    {
                        case "System.SByte":
                        case "System.Byte":
                        case "System.Int16":
                        case "System.UInt16":
                        case "System.Int32":
                        case "System.UInt32":
                            Ldc_I4((int)defaultValue);
                            break;

                        case "System.Int64":
                        case "System.UInt64":
                            Ldc_I8((long)defaultValue);
                            break;

                        case "System.Single":
                            Ldc_R4((float)defaultValue);
                            break;

                        case "System.Double":
                            Ldc_R8((double)defaultValue);
                            break;

                        case "System.String":
                            Ldstr((string)defaultValue);
                            break;

                        default:
                            if (defaultValue == null)
                            {
                                Ldnull();
                            }
                            else
                            {
                                throw new NotSupportedException(string.Format("Unknown default value literal: {0} with value of {1}.",
                                    defaultValue.GetType().FullName, defaultValue));
                            }
                            break;
                    }
                }

                #endregion
            }
            else
            {
                for (int i = 0; i < arguments.Count; i++)
                {
                    EmitArgumentForCall(arguments[i], methodParameters[i].ParameterType);
                }
            }
        }

        private void EmitArgumentForCall(IExpressionNode argument, TypeReference targetParameterType)
        {
            EmitExpressionWithTargetType(argument, targetParameterType, targetParameterType.IsByReference);
        }

        protected void Emit(ILiteralNode literal)
        {
            switch (literal.ExpressionReturnType.FullName)
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
                    throw new NotSupportedException("Unknown literal type: " + literal.ExpressionReturnType.FullName);
            }
        }

        protected void Emit(IObjectCreationNode objectCreation)
        {
            var ctor = AssemblyRegistry.GetCompatibleMethod(Assembly, objectCreation.ExpressionReturnType, ".ctor",
                objectCreation.Args.Select(argument => argument.ExpressionReturnType).ToList());

            foreach (var argument in objectCreation.Args)
            {
                Emit(argument, false);
            }

            Newobj(ctor);
        }

        protected void EmitThis()
        {
            if (methodDefinition.IsStatic)
            {
                throw new NotSupportedException("Can't emit this in a static method!");
            }

            Ldarg(0);
        }

        protected void Emit(IUnaryOperatorNode unaryOperator)
        {
            if (unaryOperator.UnaryOperatorType == UnaryOperatorNodeType.VoidOperator)
            {
                EmitVoidOperator(unaryOperator);
                return;
            }

            Emit(unaryOperator.Operand, false);

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
                    EmitStore(unaryOperator.Operand);
                    return;

                case UnaryOperatorNodeType.PostIncrement:
                    Dup();
                    Ldc_I4(1);
                    Add();
                    EmitStore(unaryOperator.Operand);
                    return;

                case UnaryOperatorNodeType.PreDecrement:
                    Ldc_I4(-1);
                    Add();
                    Dup();
                    EmitStore(unaryOperator.Operand);
                    return;

                case UnaryOperatorNodeType.PreIncrement:
                    Ldc_I4(1);
                    Add();
                    Dup();
                    EmitStore(unaryOperator.Operand);
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
                EmitAddNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand, binaryOperator.ExpressionReturnType);
            }
        }

        protected void EmitAddNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            EmitOperandsAndConvertIfNeeded(left, right);
            Add();
        }

        protected void EmitAddString(IExpressionNode left, IExpressionNode right)
        {
            Emit(left, false);

            if (left.ExpressionReturnType.IsValueType)
            {
                Box(left.ExpressionReturnType);
            }

            Emit(right, false);

            if (right.ExpressionReturnType.IsValueType)
            {
                Box(right.ExpressionReturnType);
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
                EmitGreaterEqualThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
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

        protected void EmitGreaterEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

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
                EmitGreaterThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
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

        protected void EmitGreaterThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);
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
                EmitLessEqualThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
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

        protected void EmitLessEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

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
                EmitLessThanNumeral(binaryOperator.LeftOperand, binaryOperator.RightOperand);
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

        protected void EmitLessThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

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

        private void EmitShift(IBinaryOperatorNode binaryOperator)
        {
            Emit(binaryOperator.LeftOperand, false);
            Emit(binaryOperator.RightOperand, false);

            switch (binaryOperator.BinaryOperatorType)
            {
                case BinaryOperatorNodeType.ShiftLeft:
                    Shl();
                    break;

                case BinaryOperatorNodeType.ShiftRight:
                    Shr();
                    break;

                default:
                    throw new NotSupportedException(string.Format("Unknown shift operator: {0}.", binaryOperator.BinaryOperatorType));
            }
        }

        protected void EmitVoidOperator(IUnaryOperatorNode binaryOperator)
        {
            if (binaryOperator.Operand.ExpressionType == ExpressionNodeType.AssignmentOperator)
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

        #region Store expression node

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

        protected void EmitStore(IMethodParamNode argument)
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

        protected void EmitOperandsAndConvertIfNeeded(IExpressionNode left, IExpressionNode right)
        {
            bool conversionNeeded = left.ExpressionReturnType.FullName != right.ExpressionReturnType.FullName;

            if (!conversionNeeded)
            {
                Emit(left, false);
                Emit(right, false);
            }
            else if (left.ExpressionReturnType.IsAssignableTo(right.ExpressionReturnType))
            {
                Emit(left, false);
                EmitConversionIfNeeded(left.ExpressionReturnType, right.ExpressionReturnType);
                Emit(right, false);
            }
            else if (right.ExpressionReturnType.IsAssignableTo(right.ExpressionReturnType))
            {
                Emit(left, false);
                Emit(right, false);
                EmitConversionIfNeeded(right.ExpressionReturnType, left.ExpressionReturnType);
            }
            else
            {
                throw new ArgumentException(string.Format("{0} and {1} cannot be cast to each other!", left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName));
            }
        }

        protected void EmitConversionIfNeeded(TypeReference sourceType, TypeReference targetType)
        {
            if (targetType.IsByReference)
            {
                targetType = ((ByReferenceType)targetType).ElementType;
            }

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
                if (!sourceType.DerivesFrom(targetType))
                {
                    Castclass(targetType);
                }

                return;
            }

            if (!sourceType.IsPrimitive || !targetType.IsPrimitive)
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

        private List<TempVariable> temporaryVariables = new List<TempVariable>();

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
    }
}
