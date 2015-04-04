using LaborasLangCompiler.Codegen.MethodBodyOptimizers;
using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(!Parsed, "Can't set same method twice.");

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
            Contract.Requires(!Get().HasThis, "Entry point must be static.");
            methodDefinition.DeclaringType.Module.EntryPoint = methodDefinition;
        }

        public bool IsEntryPoint()
        {
            return methodDefinition.DeclaringType.Module.EntryPoint == methodDefinition;
        }

        #region Emitters

        private void Emit(IParserNode node, bool emitReference)
        {
            Contract.Requires(node != null);
            Contract.Requires(node.Type != NodeType.ParserInternal);

            var oldSequencePoint = CurrentSequencePoint;

            if (node.SequencePoint != null)
            {
                CurrentSequencePoint = node.SequencePoint;
            }

            switch (node.Type)
            {
                case NodeType.Catch:
                    throw new NotImplementedException();

                case NodeType.CodeBlockNode:
                    Emit((ICodeBlockNode)node);
                    break;

                case NodeType.ConditionBlock:
                    Emit((IConditionBlock)node);
                    break;

                case NodeType.ExceptionHandler:
                    throw new NotImplementedException();

                case NodeType.Expression:
                    Emit((IExpressionNode)node, emitReference);
                    break;

                case NodeType.ReturnNode:
                    Emit((IReturnNode)node);
                    break;

                case NodeType.SymbolDeclaration:
                    Emit((ISymbolDeclarationNode)node);
                    break;

                case NodeType.Throw:
                    throw new NotImplementedException();

                case NodeType.WhileBlock:
                    Emit((IWhileBlockNode)node);
                    break;

                default:
                    ContractsHelper.AssertUnreachable(string.Format("Unknown IParserNode type: {0}", node.Type));
                    break;
            }

            CurrentSequencePoint = oldSequencePoint;
        }

        #region Parser node

        private void Emit(ICodeBlockNode codeBlock)
        {
            foreach (var node in codeBlock.Nodes)
            {
                Emit(node, false);
            }
        }

        private void Emit(IConditionBlock conditionBlock)
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
            Contract.Requires(expression != null);
            Contract.Requires(expression.ExpressionType != ExpressionNodeType.ParserInternal);

            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.ArrayAccess:
                    Emit((IArrayAccessNode)expression, emitReference);
                    break;

                case ExpressionNodeType.ArrayCreation:
                    Emit((IArrayCreationNode)expression);
                    break;

                case ExpressionNodeType.AssignmentOperator:
                    Emit((IAssignmentOperatorNode)expression);
                    return;

                case ExpressionNodeType.BinaryOperator:
                    Emit((IBinaryOperatorNode)expression);
                    return;

                case ExpressionNodeType.Call:
                    Emit((IFunctionCallNode)expression);
                    return;

                case ExpressionNodeType.Field:
                    Emit((IFieldNode)expression, emitReference);
                    return;

                case ExpressionNodeType.Function:
                    Emit((IMethodNode)expression);
                    return;

                case ExpressionNodeType.FunctionArgument:
                    Emit((IParameterNode)expression, emitReference);
                    return;

                case ExpressionNodeType.Literal:
                    Emit((ILiteralNode)expression);
                    return;

                case ExpressionNodeType.LocalVariable:
                    Emit((ILocalVariableNode)expression, emitReference);
                    return;

                case ExpressionNodeType.ObjectCreation:
                    Emit((IObjectCreationNode)expression);
                    return;

                case ExpressionNodeType.Property:
                    Emit((IPropertyNode)expression);
                    return;

                case ExpressionNodeType.ValueCreation:
                    throw new NotImplementedException();

                case ExpressionNodeType.This:
                    EmitThis();
                    return;

                case ExpressionNodeType.UnaryOperator:
                    Emit((IUnaryOperatorNode)expression);
                    return;

                default:
                    ContractsHelper.AssertUnreachable(string.Format("Unknown expression node type: {0}.", expression.ExpressionType));
                    return;
            }
        }

        private void Emit(IReturnNode returnNode)
        {
            if (returnNode.Expression != null)
            {
                Emit(returnNode.Expression, false);
            }

            Ret();
        }

        private void Emit(ISymbolDeclarationNode symbolDeclaration)
        {
            body.Variables.Add(symbolDeclaration.Variable);

            if (symbolDeclaration.Initializer != null)
            {
                EmitExpressionWithTargetType(symbolDeclaration.Initializer, symbolDeclaration.Variable.VariableType);
                Stloc(symbolDeclaration.Variable.Index);
            }
        }

        private void Emit(IWhileBlockNode whileBlockNode)
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


        private void EmitStore(IExpressionNode expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.ArrayAccess:
                    EmitStore((IArrayAccessNode)expression);
                    break;

                case ExpressionNodeType.Field:
                    EmitStore((IFieldNode)expression);
                    return;

                case ExpressionNodeType.FunctionArgument:
                    EmitStore((IParameterNode)expression);
                    return;

                case ExpressionNodeType.LocalVariable:
                    EmitStore((ILocalVariableNode)expression);
                    return;

                case ExpressionNodeType.Property:
                    EmitStore((IPropertyNode)expression);
                    return;

                case ExpressionNodeType.UnaryOperator:
                    EmitStore((IUnaryOperatorNode)expression);
                    return;

                default:
                    ContractsHelper.AssumeUnreachable(string.Format("Cannot store {0} ExpressionNode.", expression.ExpressionType));
                    return;
            }
        }

        #region Load expression node

        private void Emit(IFieldNode field, bool emitReference)
        {
            emitReference &= field.Field.FieldType.IsValueType;

            if (!field.Field.Resolve().IsStatic)
            {
                Contract.Assume(field.ObjectInstance != null);
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

        private void Emit(IParameterNode argument, bool emitReference)
        {
            Contract.Assume(argument.Parameter.Index >= 0);

            var index = argument.Parameter.Index + (methodDefinition.HasThis ? 1 : 0);
            emitReference &= argument.Parameter.ParameterType.IsValueType;

            if (emitReference)
            {
                Ldarga(index);
            }
            else
            {
                Ldarg(index);
            }
        }

        private void Emit(ILocalVariableNode variable, bool emitReference)
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

        private void Emit(IPropertyNode property)
        {
            var getter = AssemblyRegistry.GetPropertyGetter(Assembly, property.Property);

            if (getter.HasThis)
            {
                Contract.Assume(property.ObjectInstance != null);
                Emit(property.ObjectInstance, true);
            }

            Call(getter);
        }

        private void Emit(IArrayAccessNode arrayAccess, bool emitReference)
        {
            var array = arrayAccess.Array;
            var arrayType = array.ExpressionReturnType as ArrayType;
            var indices = arrayAccess.Indices;

            Emit(arrayAccess.Array, true);

            if (arrayType != null)
            {
                emitReference &= array.ExpressionReturnType.IsValueType;

                if (arrayType.IsVector)
                {
                    Contract.Assume(indices.Count == 1);
                    Emit(indices[0], false);

                    if (emitReference)
                    {
                        Ldelema(arrayType.ElementType);
                    }
                    else
                    {
                        Ldelem(arrayType.ElementType);
                    }
                }
                else
                {
                    var loadElementMethod = emitReference ? AssemblyRegistry.GetArrayLoadElementAddress(arrayType) : AssemblyRegistry.GetArrayLoadElement(arrayType);
                    EmitArgumentsForCall(indices, loadElementMethod);
                    Call(loadElementMethod);
                }
            }
            else
            {
                var getter = AssemblyRegistry.GetCompatibleMethod(Assembly, array.ExpressionReturnType, "get_Item", indices.Select(index => index.ExpressionReturnType).ToArray());
                Contract.Assume(getter != null);

                for (int i = 0; i < indices.Count; i++)
                {
                    Emit(indices[i], false);
                }

                Call(getter);
            }
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
        private void Emit(IAssignmentOperatorNode assignmentOperator, bool duplicateValueInStack = true)
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
                    Contract.Assume(fieldNode.ObjectInstance != null);

                    memberHasThis = true;
                    objectInstance = fieldNode.ObjectInstance;
                }
            }
            else if (isProperty)
            {
                var propertyNode = (IPropertyNode)assignmentOperator.LeftOperand;
                var property = propertyNode.Property.Resolve();

                Contract.Assume(property.SetMethod != null);

                if (property.SetMethod.HasThis)
                {
                    Contract.Assume(propertyNode.ObjectInstance != null);

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
                    tempVariable = temporaryVariables.Acquire(assignmentOperator.LeftOperand.ExpressionReturnType);

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
                    temporaryVariables.Release(tempVariable);
                }
                else
                {
                    Emit(assignmentOperator.LeftOperand, false);
                }
            }
        }

        private void EmitExpressionWithTargetType(IExpressionNode expression, TypeReference targetType, bool emitAsReference = false)
        {
            Contract.Requires(!emitAsReference || CanEmitAsReference(expression), "Can't pass RValue by reference.");

            bool expressionIsFunction = expression.ExpressionType == ExpressionNodeType.Function;
            bool expressionIsFunctor = expression.ExpressionReturnType.IsFunctorType();

            var targetBaseType = targetType.Resolve().BaseType;
            bool targetIsDelegate = targetBaseType != null && targetBaseType.FullName == "System.MulticastDelegate";

            // We'll want to emit expression in all cases
            Emit(expression, emitAsReference);

            if (targetIsDelegate)
            {
                // Sanity check
                Contract.Assert(!expressionIsFunction || !expressionIsFunctor);

                if (expressionIsFunctor)
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

        private void Emit(IBinaryOperatorNode binaryOperator)
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
                    ContractsHelper.AssumeUnreachable(string.Format("Unknown binary operator node: {0}", binaryOperator.BinaryOperatorType));
                    return;
            }
        }

        private void Emit(IMethodNode function)
        {
            var returnTypeIsDelegate = function.ExpressionReturnType.Resolve().BaseType.FullName == "System.MulticastDelegate";

            if (!returnTypeIsDelegate)
            {
                var functorType = AssemblyRegistry.GetImplementationFunctorType(Assembly, DeclaringType, function.Method);
                var ctor = AssemblyRegistry.GetMethod(Assembly, functorType, ".ctor");

                if (function.Method.HasThis)
                {
                    Contract.Assume(function.ObjectInstance != null);
                    Emit(function.ObjectInstance, false);
                }

                Newobj(ctor);
            }
            else
            {
                var ctor = AssemblyRegistry.GetMethod(Assembly, function.ExpressionReturnType, ".ctor");

                if (function.Method.HasThis)
                {
                    Contract.Assume(function.ObjectInstance != null);
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

        private void Emit(IFunctionCallNode functionCall)
        {
            var function = functionCall.Function;

            if (function.ExpressionType == ExpressionNodeType.Function)
            {
                // Direct call
                var functionNode = (IMethodNode)function;

                if (functionNode.Method.HasThis)
                {
                    Contract.Assume(functionNode.ObjectInstance != null);
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
            {
                // Functor Call
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

            if (resolvedMethod != null && resolvedMethod.IsParamsMethod())
            {
                EmitArgumentsForParamsCall(arguments, methodParameters);
            }
            else if (resolvedMethod != null && methodParameters.Count > arguments.Count)    // Method with optional parameters call
            {
                EmitArgumentsForCallWithOptionalParameters(arguments, methodParameters, resolvedMethod);
            }
            else
            {
                for (int i = 0; i < arguments.Count; i++)
                {
                    EmitArgumentForCall(arguments[i], methodParameters[i].ParameterType);
                }
            }
        }

        private void EmitArgumentsForParamsCall(IReadOnlyList<IExpressionNode> arguments, IList<ParameterDefinition> methodParameters)
        {
            for (int i = 0; i < methodParameters.Count - 1; i++)
            {
                EmitArgumentForCall(arguments[i], methodParameters[i].ParameterType);
            }

            var arrayVariable = temporaryVariables.Acquire(methodParameters.Last().ParameterType);
            var elementType = methodParameters.Last().ParameterType.GetElementType();

            Ldc_I4(arguments.Count - methodParameters.Count + 1);
            Newarr(elementType);
            Stloc(arrayVariable.Index);

            for (int i = methodParameters.Count - 1; i < arguments.Count; i++)
            {
                Ldloc(arrayVariable.Index);
                Ldc_I4(i - methodParameters.Count + 1);
                EmitArgumentForCall(arguments[i], elementType);
                Stelem(elementType);
            }

            Ldloc(arrayVariable.Index);
            temporaryVariables.Release(arrayVariable);
        }

        private void EmitArgumentsForCallWithOptionalParameters(IReadOnlyList<IExpressionNode> arguments, IList<ParameterDefinition> methodParameters, MethodDefinition resolvedMethod)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                EmitArgumentForCall(arguments[i], methodParameters[i].ParameterType);
            }

            for (int i = arguments.Count; i < methodParameters.Count; i++)
            {
                // Use resolved method here for getting constant value, as it will not be present in method reference parameters.
                // Furthermore, we must not reference resolved method parameter TYPES as they will be resolved as well
                var defaultValue = resolvedMethod.Parameters[i].Constant;

                if (defaultValue == null)
                {
                    Ldnull();
                    continue;
                }

                var constantType = defaultValue.GetType();

                if (constantType == typeof(SByte) ||
                    constantType == typeof(Byte) ||
                    constantType == typeof(Int16) ||
                    constantType == typeof(UInt16) ||
                    constantType == typeof(Int32) ||
                    constantType == typeof(UInt32))
                {
                    Ldc_I4((int)defaultValue);
                }
                else if (constantType == typeof(Int64) ||
                         constantType == typeof(UInt64))
                {
                    Ldc_I8((long)defaultValue);
                }
                else if (constantType == typeof(float))
                {
                    Ldc_R4((float)defaultValue);
                }
                else if (constantType == typeof(double))
                {
                    Ldc_R8((double)defaultValue);
                }
                else if (constantType == typeof(string))
                {
                    Ldstr((string)defaultValue);
                }
                else
                {
                    ContractsHelper.AssumeUnreachable(string.Format("Unknown default value literal: {0} with value of {1}.",
                            defaultValue.GetType().FullName, defaultValue));
                }
            }
        }

        private void EmitArgumentForCall(IExpressionNode argument, TypeReference targetParameterType)
        {
            EmitExpressionWithTargetType(argument, targetParameterType, targetParameterType.IsByReference);
        }

        private void Emit(ILiteralNode literal)
        {
            switch (literal.ExpressionReturnType.MetadataType)
            {
                case MetadataType.Boolean:
                    Ldc_I4((bool)literal.Value ? 1 : 0);
                    return;

                case MetadataType.SByte:
                    Ldc_I4((sbyte)literal.Value);
                    return;

                case MetadataType.Int16:
                    Ldc_I4((short)literal.Value);
                    return;

                case MetadataType.Int32:
                    Ldc_I4((int)literal.Value);
                    return;

                case MetadataType.Int64:
                    Ldc_I8((long)literal.Value);
                    return;

                case MetadataType.Byte:
                    Ldc_I4((byte)literal.Value);
                    return;

                case MetadataType.UInt16:
                    Ldc_I4((ushort)literal.Value);
                    return;

                case MetadataType.Char:
                    Ldc_I4((char)literal.Value);
                    return;

                // first cast is checked
                case MetadataType.UInt32:
                    Ldc_I4((int)(uint)literal.Value);
                    return;

                // first cast is checked
                case MetadataType.UInt64:
                    Ldc_I8((long)(ulong)literal.Value);
                    return;

                case MetadataType.Single:
                    Ldc_R4((float)literal.Value);
                    return;

                case MetadataType.Double:
                    Ldc_R8((double)literal.Value);
                    return;

                case MetadataType.String:
                    Ldstr((string)literal.Value);
                    return;

                default:
                    ContractsHelper.AssertUnreachable("Unknown literal type: " + literal.ExpressionReturnType.FullName);
                    return;
            }
        }

        private void Emit(IObjectCreationNode objectCreation)
        {
            foreach (var argument in objectCreation.Args)
            {
                Emit(argument, false);
            }

            Newobj(objectCreation.Constructor);
        }

        private void Emit(IArrayCreationNode arrayCreation)
        {
            Contract.Requires(arrayCreation.ExpressionReturnType.IsArray, "Return type of IArrayCreationNode must be an array type.");
            Contract.Requires(arrayCreation.Dimensions.Count == ((ArrayType)arrayCreation.ExpressionReturnType).Rank, "Array creation node dimension count must match array type rank.");

            var arrayType = (ArrayType)arrayCreation.ExpressionReturnType;

            if (arrayType.IsVector)
            {
                Emit(arrayCreation.Dimensions[0], false);
                Newarr(arrayType.ElementType);
            }
            else
            {
                var rank = arrayType.Rank;
                var constructor = AssemblyRegistry.GetArrayConstructor(arrayType);

                for (int i = 0; i < rank; i++)
                {
                    Emit(arrayCreation.Dimensions[i], false);
                }

                Newobj(constructor);
            }

            if (arrayCreation.Initializer != null)
            {
                if (CanEmitArrayInitializerFastPath(arrayCreation.Initializer))
                {
                    EmitInitializerFastPath(arrayType, arrayCreation.Initializer);
                }
                else if (arrayType.IsVector)
                {
                    EmitVectorInitializerSlowPath(arrayType, arrayCreation.Initializer);
                }
                else
                {
                    EmitArrayInitializerSlowPath(arrayCreation);
                }
            }
        }

        [Pure]
        private bool CanEmitArrayInitializerFastPath(IReadOnlyList<IExpressionNode> initializer)
        {
            for (int i = 0; i < initializer.Count; i++)
            {
                if (initializer[i].ExpressionType != ExpressionNodeType.Literal || !initializer[i].ExpressionReturnType.IsValueType)
                    return false;
            }

            return true;
        }

        // Assumes array is on the stack but it must leave it on the stack after the function is done
        private void EmitInitializerFastPath(ArrayType arrayType, IReadOnlyList<IExpressionNode> initializer)
        {
            Contract.Requires(CanEmitArrayInitializerFastPath(initializer));

            var field = AssemblyRegistry.GetArrayInitializerField(Assembly, arrayType.ElementType, initializer);
            var initializeArrayMethod = AssemblyRegistry.GetMethod(Assembly, "System.Runtime.CompilerServices.RuntimeHelpers", "InitializeArray");

            Dup();
            Ldtoken(field);
            Call(initializeArrayMethod);
        }

        // Assumes array is on the stack but it must leave it on the stack after the function is done
        private void EmitVectorInitializerSlowPath(ArrayType arrayType, IReadOnlyList<IExpressionNode> initializer)
        {
            Contract.Requires(arrayType.IsVector);

            var oldSequencePoint = CurrentSequencePoint;

            for (int i = 0; i < initializer.Count; i++)
            {
                CurrentSequencePoint = initializer[i].SequencePoint;

                Dup();
                Ldc_I4(i);
                EmitExpressionWithTargetType(initializer[i], arrayType.ElementType, false);
                Stelem(arrayType.ElementType);
            }

            CurrentSequencePoint = oldSequencePoint;
        }

        // Assumes array is on the stack but it must leave it on the stack after the function is done
        private void EmitArrayInitializerSlowPath(IArrayCreationNode arrayCreation)
        {
            Contract.Requires(arrayCreation.ExpressionReturnType is ArrayType);
            Contract.Requires(!((ArrayType)arrayCreation.ExpressionReturnType).IsVector);

            var oldSequencePoint = CurrentSequencePoint;

            var arrayType = (ArrayType)arrayCreation.ExpressionReturnType;
            var storeElementMethod = AssemblyRegistry.GetArrayStoreElement(arrayType);
            var initializer = arrayCreation.Initializer;

            var dimensionCount = arrayCreation.Dimensions.Count;
            var dimensions = new int[dimensionCount];
            var indexSizes = new int[dimensionCount];

            for (int i = dimensionCount - 1; i > -1; i--)
            {
                Contract.Assume(arrayCreation.Dimensions[i] is ILiteralNode);

                dimensions[i] = (int)((ILiteralNode)arrayCreation.Dimensions[i]).Value;
                indexSizes[i] = i != dimensionCount - 1 ? indexSizes[i + 1] * dimensions[i + 1] : 1;
            }

            for (int i = 0; i < initializer.Count; i++)
            {
                CurrentSequencePoint = initializer[i].SequencePoint;

                Dup();

                for (int j = 0; j < dimensionCount; j++)
                {
                    Ldc_I4((i / indexSizes[j]) % dimensions[j]);
                }

                EmitExpressionWithTargetType(initializer[i], arrayType.ElementType, false);
                Call(storeElementMethod);
            }

            CurrentSequencePoint = oldSequencePoint;
        }

        private void EmitThis()
        {
            Contract.Requires(!methodDefinition.IsStatic);
            Ldarg(0);
        }

        private void Emit(IUnaryOperatorNode unaryOperator)
        {
            if (unaryOperator.UnaryOperatorType == UnaryOperatorNodeType.VoidOperator)
            {
                EmitVoidOperator(unaryOperator);
                return;
            }

            Emit(unaryOperator.Operand, false);

            Contract.Assume(unaryOperator.UnaryOperatorType != UnaryOperatorNodeType.VoidOperator);

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
                    ContractsHelper.AssertUnreachable(string.Format("Unknown unary operator type: {0}", unaryOperator.UnaryOperatorType));
                    return;
            }
        }

        #region Add emitter

        private void EmitAdd(IBinaryOperatorNode binaryOperator)
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

        private void EmitAddNumeral(IExpressionNode left, IExpressionNode right, TypeReference resultType)
        {
            EmitOperandsAndConvertIfNeeded(left, right);
            Add();
        }

        private void EmitAddString(IExpressionNode left, IExpressionNode right)
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

            var concatMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, Assembly.TypeSystem.String, "Concat", new TypeReference[]
                {
                    Assembly.TypeSystem.Object,
                    Assembly.TypeSystem.Object
                });

            Call(concatMethod);
        }

        #endregion

        #region Logical And/Logical Or emitters

        private void EmitLogicalAnd(IBinaryOperatorNode binaryOperator)
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

        private void EmitLogicalOr(IBinaryOperatorNode binaryOperator)
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

        private void EmitGreaterEqualThan(IBinaryOperatorNode binaryOperator)
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

        private void EmitGreaterEqualThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, Assembly.TypeSystem.String, "CompareOrdinal",
                new TypeReference[]
                {
                    Assembly.TypeSystem.String,
                    Assembly.TypeSystem.String
                });

            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Clt();

            Ldc_I4(0);
            Ceq();
        }

        private void EmitGreaterEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            Clt();
            Ldc_I4(0);
            Ceq();
        }

        #endregion

        #region Greater than emitter

        private void EmitGreaterThan(IBinaryOperatorNode binaryOperator)
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

        private void EmitGreaterThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, Assembly.TypeSystem.String, "CompareOrdinal",
                new TypeReference[]
                {
                    Assembly.TypeSystem.String,
                    Assembly.TypeSystem.String
                });


            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Cgt();
        }

        private void EmitGreaterThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);
            Cgt();
        }

        #endregion

        #region Less equal than emitter

        private void EmitLessEqualThan(IBinaryOperatorNode binaryOperator)
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

        private void EmitLessEqualThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, Assembly.TypeSystem.String, "CompareOrdinal",
                new TypeReference[]
                {
                    Assembly.TypeSystem.String,
                    Assembly.TypeSystem.String
                });


            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Cgt();

            Ldc_I4(0);
            Ceq();
        }

        private void EmitLessEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            Cgt();
            Ldc_I4(0);
            Ceq();
        }

        #endregion

        #region Emit less than

        private void EmitLessThan(IBinaryOperatorNode binaryOperator)
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

        private void EmitLessThanString(IExpressionNode left, IExpressionNode right)
        {
            var stringComparisonMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, Assembly.TypeSystem.String, "CompareOrdinal",
                new TypeReference[]
                {
                    Assembly.TypeSystem.String,
                    Assembly.TypeSystem.String
                });


            Emit(left, false);
            Emit(right, false);

            Call(stringComparisonMethod);

            Ldc_I4(0);
            Clt();
        }

        private void EmitLessThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            Clt();
        }

        #endregion

        #endregion

        #region Division/Remainder emitters

        private void EmitDivision(IBinaryOperatorNode binaryOperator)
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

        private void EmitRemainder(IBinaryOperatorNode binaryOperator)
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
            Contract.Requires(binaryOperator.BinaryOperatorType == BinaryOperatorNodeType.ShiftLeft || binaryOperator.BinaryOperatorType == BinaryOperatorNodeType.ShiftRight);

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
                    ContractsHelper.AssumeUnreachable(string.Format("Unknown shift operator: {0}.", binaryOperator.BinaryOperatorType));
                    break;
            }
        }

        private void EmitVoidOperator(IUnaryOperatorNode unaryOperator)
        {
            if (unaryOperator.Operand.ExpressionType == ExpressionNodeType.AssignmentOperator)
            {
                Emit(((IAssignmentOperatorNode)unaryOperator.Operand), false);
            }
            else
            {
                Emit(unaryOperator.Operand, false);
                Pop();
            }
        }

        #endregion

        #region Store expression node

        private void EmitStore(IFieldNode field)
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

        private void EmitStore(IParameterNode argument)
        {
            Starg(argument.Parameter.Index);
        }

        private void EmitStore(ILocalVariableNode variable)
        {
            Stloc(variable.LocalVariable.Index);
        }

        private void EmitStore(IPropertyNode property)
        {
            var setter = AssemblyRegistry.GetPropertySetter(Assembly, property.Property);
            Call(setter);
        }

        private void EmitStore(IArrayAccessNode arrayAccess)
        {
            var array = arrayAccess.Array;
            var arrayType = array.ExpressionReturnType as ArrayType;
            var indices = arrayAccess.Indices;
            
            var valueVariable = temporaryVariables.Acquire(arrayAccess.ExpressionReturnType);
            Stloc(valueVariable.Index);

            Emit(arrayAccess.Array, true);

            if (arrayType != null)
            {
                if (arrayType.IsVector)
                {
                    Contract.Assume(indices.Count == 1);
                    Emit(indices[0], false);
                    Ldloc(valueVariable.Index);
                    Stelem(arrayType.ElementType);
                }
                else
                {
                    var storeElementMethod = AssemblyRegistry.GetArrayStoreElement(arrayType);

                    for (int i = 0; i < indices.Count; i++)
                    {
                        EmitArgumentForCall(indices[i], storeElementMethod.Parameters[i].ParameterType);
                    }

                    Ldloc(valueVariable.Index);
                    Call(storeElementMethod);
                }
            }
            else
            {
                var setter = AssemblyRegistry.GetCompatibleMethod(Assembly, array.ExpressionReturnType, "set_Item", indices.Select(index => index.ExpressionReturnType).ToArray());
                Contract.Assume(setter != null);

                for (int i = 0; i < indices.Count; i++)
                {
                    Emit(indices[i], false);
                }

                Ldloc(valueVariable.Index);
                Call(setter);
            }

            temporaryVariables.Release(valueVariable);
        }

        private void EmitStore(IUnaryOperatorNode unaryOperatorNode)
        {
            switch (unaryOperatorNode.UnaryOperatorType)
            {
                case UnaryOperatorNodeType.PreDecrement:
                case UnaryOperatorNodeType.PreIncrement:
                    EmitStore(unaryOperatorNode.Operand);
                    return;
            }

            ContractsHelper.AssertUnreachable(string.Format("Cannot store unary operator {0}.", unaryOperatorNode.UnaryOperatorType));
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
                ContractsHelper.AssumeUnreachable(string.Format("{0} and {1} cannot be cast to each other!", left.ExpressionReturnType.FullName, right.ExpressionReturnType.FullName));
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

            if (!sourceType.IsValueType)
            {
                if (!sourceType.DerivesFrom(targetType))
                {
                    Castclass(targetType);
                }

                return;
            }

            Contract.Assert(sourceType.IsPrimitive && targetType.IsPrimitive);

            switch (targetType.MetadataType)
            {
                case MetadataType.SByte:
                    Conv_I1();
                    break;

                case MetadataType.Int16:
                    Conv_I2();
                    break;

                case MetadataType.Int32:
                    Conv_I4();
                    break;

                case MetadataType.Int64:
                    Conv_I8();
                    break;

                case MetadataType.IntPtr:
                    Conv_I();
                    break;

                case MetadataType.Byte:
                    Conv_U1();
                    break;

                case MetadataType.UInt16:
                    Conv_U2();
                    break;

                case MetadataType.UInt32:
                    Conv_U4();
                    break;

                case MetadataType.UInt64:
                    Conv_U8();
                    break;

                case MetadataType.UIntPtr:
                    Conv_U();
                    break;

                case MetadataType.Single:
                    if (sourceType.IsUnsignedInteger())
                    {
                        Conv_R_Un();
                    }
                    else
                    {
                        Conv_R4();
                    }
                    break;

                case MetadataType.Double:
                    Conv_R8();
                    break;

                default:
                    ContractsHelper.AssumeUnreachable(string.Format("Unknown primitive type: {0}", targetType.FullName));
                    break;
            }
        }

        #endregion
    }
}
