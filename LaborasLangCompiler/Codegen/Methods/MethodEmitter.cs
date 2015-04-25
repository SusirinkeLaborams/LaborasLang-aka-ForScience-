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

        protected enum EmissionType
        {
            None,
            Value,
            ThisArg,
            ReferenceToValue
        }

        private void Emit(IParserNode node)
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
                    Emit((IExpressionNode)node, EmissionType.None);
                    break;

                case NodeType.ForLoop:
                    Emit((IForLoopNode)node);
                    break;

                case NodeType.ForEachLoop:
                    Emit((IForEachLoopNode)node);
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
            temporaryVariables.ReleaseAll();
        }

        #region Parser node

        private void Emit(ICodeBlockNode codeBlock)
        {
            foreach (var node in codeBlock.Nodes)
            {
                Emit(node);
            }
        }

        private void Emit(IConditionBlock conditionBlock)
        {
            var elseBlockLabel = CreateLabel();
            var end = CreateLabel();

            Emit(conditionBlock.Condition, EmissionType.Value);

            if (conditionBlock.FalseBlock != null)
            {
                Brfalse(elseBlockLabel);
            }
            else
            {
                Brfalse(end);
            }

            Emit(conditionBlock.TrueBlock);

            if (conditionBlock.FalseBlock != null)
            {
                Br(end);
                Emit(elseBlockLabel);
                Emit(conditionBlock.FalseBlock);
            }

            Emit(end);
        }

        protected void Emit(IExpressionNode expression, EmissionType emissionType)
        {
            Contract.Requires(expression != null);
            Contract.Requires(expression.ExpressionType != ExpressionNodeType.ParserInternal);

            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.ArrayAccess:
                    Emit((IArrayAccessNode)expression, emissionType);
                    return;

                case ExpressionNodeType.ArrayCreation:
                    Emit((IArrayCreationNode)expression, emissionType);
                    return;

                case ExpressionNodeType.AssignmentOperator:
                    Emit((IAssignmentOperatorNode)expression, emissionType);
                    return;

                case ExpressionNodeType.BinaryOperator:
                    Emit((IBinaryOperatorNode)expression, emissionType);
                    return;

                case ExpressionNodeType.Call:
                    Emit((IFunctionCallNode)expression, emissionType);
                    return;
                    
                case ExpressionNodeType.Cast:
                    Emit((ICastNode)expression, emissionType);
                    return;

                case ExpressionNodeType.Field:
                    Emit((IFieldNode)expression, emissionType);
                    return;

                case ExpressionNodeType.Function:
                    Emit((IMethodNode)expression, emissionType);
                    return;

                case ExpressionNodeType.FunctionArgument:
                    Emit((IParameterNode)expression, emissionType);
                    return;

                case ExpressionNodeType.IncrementDecrementOperator:
                    Emit((IIncrementDecrementOperatorNode)expression, emissionType);
                    return;

                case ExpressionNodeType.IndexOperator:
                    Emit((IIndexOperatorNode)expression, emissionType);
                    return;

                case ExpressionNodeType.Literal:
                    Emit((ILiteralNode)expression, emissionType);
                    return;

                case ExpressionNodeType.LocalVariable:
                    Emit((ILocalVariableNode)expression, emissionType);
                    return;

                case ExpressionNodeType.Null:
                    EmitNull(expression, emissionType);
                    return;

                case ExpressionNodeType.ObjectCreation:
                    Emit((IObjectCreationNode)expression, emissionType);
                    return;

                case ExpressionNodeType.Property:
                    Emit((IPropertyNode)expression, emissionType);
                    return;

                case ExpressionNodeType.ValueCreation:
                    EmitValueCreation(expression, emissionType);
                    return;

                case ExpressionNodeType.This:
                    EmitThis(expression, emissionType);
                    return;

                case ExpressionNodeType.UnaryOperator:
                    Emit((IUnaryOperatorNode)expression, emissionType);
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
                EmitExpressionWithTargetType(returnNode.Expression, methodDefinition.ReturnType);
            }

            Ret();
        }

        private void Emit(ISymbolDeclarationNode symbolDeclaration)
        {
            body.Variables.Add(symbolDeclaration.Variable);
            Assembly.AddTypeUsage(symbolDeclaration.Variable.VariableType);

            if (symbolDeclaration.Initializer != null && symbolDeclaration.Initializer.ExpressionType != ExpressionNodeType.ValueCreation)
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

            Emit(whileBlockNode.Condition, EmissionType.Value);
            Brfalse(loopEnd);

            Emit(whileBlockNode.ExecutedBlock);
            Br(loopStart);

            Emit(loopEnd);
        }

        private void Emit(IForLoopNode forLoop)
        {
            var loopBody = CreateLabel();
            var loopCondition = CreateLabel();

            Emit(forLoop.InitializationBlock);
            Br(loopCondition);

            Emit(loopBody);
            Emit(forLoop.Body);
            Emit(forLoop.IncrementBlock);

            Emit(loopCondition);
            Emit(forLoop.ConditionBlock, EmissionType.Value);
            Brtrue(loopBody);
        }

        private void Emit(IForEachLoopNode forEachLoop)
        {
            var collectionType = forEachLoop.Collection.ExpressionReturnType;
            var arrayType = collectionType as ArrayType;

            if (arrayType != null)
            {
                if (arrayType.IsVector)
                {
                    EmitForEachLoopForVector(forEachLoop);
                }
                else
                {
                    EmitForEachLoopForArray(forEachLoop);
                }
            }
            else
            {
                EmitForEachLoopForCollection(forEachLoop);
            }
        }

        private void EmitForEachLoopForVector(IForEachLoopNode forEachLoop)
        {
            Contract.Requires(forEachLoop.LoopVariable.Initializer == null);
            Contract.Requires(forEachLoop.Collection.ExpressionReturnType is ArrayType);

            var loopVariable = forEachLoop.LoopVariable.Variable;
            var collectionType = (ArrayType)forEachLoop.Collection.ExpressionReturnType;
            VariableDefinition collectionVariable;

            // Store array in a local variable
            var collectionLocalVariableNode = forEachLoop.Collection as ILocalVariableNode;
            if (collectionLocalVariableNode != null)
            {
                collectionVariable = collectionLocalVariableNode.LocalVariable;
            }
            else
            {
                collectionVariable = temporaryVariables.Acquire(forEachLoop.Collection.ExpressionReturnType);
                Emit(forEachLoop.Collection, EmissionType.Value);
                Stloc(collectionVariable.Index);
            }

            // Store array length in a local variable
            var arrayLengthVariable = temporaryVariables.Acquire(Assembly.TypeSystem.UIntPtr);
            EmitLocalVariable(collectionVariable, EmissionType.ThisArg);
            Ldlen();
            Stloc(arrayLengthVariable.Index);

            // Store 0 in index variable
            var indexVariable = temporaryVariables.Acquire(Assembly.TypeSystem.UIntPtr);
            Ldc_I4(0);
            Stloc(indexVariable.Index);

            // At this point, we have the array inside collectionVariable, its length in arrayLengthVariable and 0 in index variable

            var loopBody = CreateLabel();
            var loopCondition = CreateLabel();
            
            // Declare loop variable
            Emit(forEachLoop.LoopVariable);
            Br(loopCondition);

            // Load i-th item from the array
            Emit(loopBody);
            EmitLocalVariable(collectionVariable, EmissionType.ThisArg);
            EmitLocalVariable(indexVariable, EmissionType.Value);
            Ldelem(collectionType.ElementType);
            EmitConversionIfNeeded(collectionType.ElementType, loopVariable.VariableType);
            Stloc(loopVariable.Index);

            // Emit loop body
            Emit(forEachLoop.Body);

            // Increment index
            EmitLocalVariable(indexVariable, EmissionType.Value);
            Ldc_I4(1);
            Add();
            Stloc(indexVariable.Index);
            
            // Emit loop condition
            Emit(loopCondition);
            EmitLocalVariable(indexVariable, EmissionType.Value);
            EmitLocalVariable(arrayLengthVariable, EmissionType.Value);
            Blt_Un(loopBody);

            temporaryVariables.Release(indexVariable);
            temporaryVariables.Release(arrayLengthVariable);

            if (collectionLocalVariableNode != null)
            {
                temporaryVariables.Release(collectionVariable);
            }
        }

        private void EmitForEachLoopForArray(IForEachLoopNode forEachLoop)
        {
            Contract.Requires(forEachLoop.LoopVariable.Initializer == null);
            Contract.Requires(forEachLoop.Collection.ExpressionReturnType is ArrayType);

            var loopVariable = forEachLoop.LoopVariable.Variable;
            var collectionType = (ArrayType)forEachLoop.Collection.ExpressionReturnType;
            var getLengthMethod = AssemblyRegistry.GetMethod(Assembly, collectionType, "get_Length");
            var addressMethod = AssemblyRegistry.GetArrayLoadElementAddress(collectionType);
            
            var referenceType = new ByReferenceType(collectionType.ElementType);
            var pinnedType = new PinnedType(referenceType);
            var ptrType = new PointerType(collectionType.ElementType);

            var pinnedArrayStartVariable = temporaryVariables.Acquire(pinnedType);
            var arrayEndVariable = temporaryVariables.Acquire(ptrType);
            var ptrVariable = temporaryVariables.Acquire(ptrType);

            int elementSize;
            bool elementSizeKnown = MetadataHelpers.GetSizeOfType(collectionType.ElementType, out elementSize);
            VariableDefinition elementSizeVariable = null;

            if (!elementSizeKnown)
            {
                elementSizeVariable = temporaryVariables.Acquire(Assembly.TypeSystem.UInt32);
                Sizeof(collectionType.ElementType);
                Stloc(elementSizeVariable.Index);
            }
            
            Emit(forEachLoop.Collection, EmissionType.ThisArg);
            Dup();

            for (int i = 0; i < collectionType.Rank; i++)
                Ldc_I4(0);

            Call(forEachLoop.Collection.ExpressionReturnType, addressMethod);
            Stloc(pinnedArrayStartVariable.Index);

            Call(forEachLoop.Collection.ExpressionReturnType, getLengthMethod);

            if (elementSizeKnown)
            {
                Ldc_I4(elementSize);
            }
            else
            {
                EmitLocalVariable(elementSizeVariable, EmissionType.Value);
            }

            Mul();
            Conv_I();
            EmitLocalVariable(pinnedArrayStartVariable, EmissionType.Value);
            Conv_I();
            Dup();
            Stloc(ptrVariable.Index);

            Add();
            Stloc(arrayEndVariable.Index);
            
            // At this point, we have the pinned array start in PinnedArrayStartVariable and ptrVariable, and its end address in arrayEndVariable

            var loopBody = CreateLabel();
            var loopCondition = CreateLabel();

            // Declare loop variable
            Emit(forEachLoop.LoopVariable);
            Br(loopCondition);

            // Load i-th item from the array
            Emit(loopBody);
            EmitLocalVariable(ptrVariable, EmissionType.Value);
            Ldind(collectionType.ElementType);
            EmitConversionIfNeeded(collectionType.ElementType, loopVariable.VariableType);
            Stloc(loopVariable.Index);

            // Emit loop body
            Emit(forEachLoop.Body);

            // Increment index
            EmitLocalVariable(ptrVariable, EmissionType.Value);

            if (elementSizeKnown)
            {
                Ldc_I4(elementSize);
            }
            else
            {
                EmitLocalVariable(elementSizeVariable, EmissionType.Value);
            }

            Add();
            Stloc(ptrVariable.Index);

            // Emit loop condition
            Emit(loopCondition);
            EmitLocalVariable(ptrVariable, EmissionType.Value);
            EmitLocalVariable(arrayEndVariable, EmissionType.Value);
            Bne_Un(loopBody);
            
            // Store 0 into pinned local variable
            Ldc_I4(0);
            Conv_U();
            Stloc(pinnedArrayStartVariable.Index);

            if (!elementSizeKnown)
            {
                temporaryVariables.Release(elementSizeVariable);
            }

            temporaryVariables.Release(ptrVariable);
            temporaryVariables.Release(arrayEndVariable);
            temporaryVariables.Release(pinnedArrayStartVariable);
        }

        private void EmitForEachLoopForCollection(IForEachLoopNode forEachLoop)
        {
            Contract.Requires(forEachLoop.LoopVariable.Initializer == null);

            var loopVariable = forEachLoop.LoopVariable.Variable;
            var getEnumeratorMethod = AssemblyRegistry.GetGetEnumeratorMethod(forEachLoop.Collection.ExpressionReturnType, loopVariable.VariableType);
            Contract.Assume(getEnumeratorMethod != null);

            var enumeratorType = getEnumeratorMethod.GetReturnType();
            var moveNextMethod = AssemblyRegistry.GetEnumeratorMoveNextMethod(enumeratorType, loopVariable.VariableType);
            var getCurrentMethod = AssemblyRegistry.GetEnumeratorCurrentMethod(enumeratorType, loopVariable.VariableType);
            var idisposable = AssemblyRegistry.FindType(Assembly, "System.IDisposable");
            bool isDisposable = enumeratorType.DerivesFrom(idisposable);
            bool shouldAttemptToDispose = !enumeratorType.IsValueType || isDisposable;

            Instruction beginTry = null, endTry = null, endFinally = null;

            // Define labels for later use
            var loopStart = CreateLabel();
            var loopCondition = CreateLabel();

            if (shouldAttemptToDispose)
            {
                beginTry = CreateLabel();
                endTry = CreateLabel();
                endFinally = CreateLabel();
            }

            // Declare loop variable
            Emit(forEachLoop.LoopVariable);

            // Claim temporary variable for enumerator
            var enumeratorVariable = temporaryVariables.Acquire(enumeratorType);

            // enumerator = collection.GetEnumerator()
            Emit(forEachLoop.Collection, EmissionType.ThisArg);
            Call(forEachLoop.Collection.ExpressionReturnType, getEnumeratorMethod);
            Stloc(enumeratorVariable.Index);

            // try
            {
                if (shouldAttemptToDispose)
                {
                    Emit(beginTry);
                }

                // Jump to condition block
                Br(loopCondition);

                // loopVariable = enumerator.get_Current()
                Emit(loopStart);
                EmitLocalVariable(enumeratorVariable, EmissionType.ThisArg);
                Call(enumeratorVariable.VariableType, getCurrentMethod);
                EmitConversionIfNeeded(getCurrentMethod.GetReturnType(), loopVariable.VariableType);
                Stloc(loopVariable.Index);

                // Emit loop body
                Emit(forEachLoop.Body);

                // Emit condition block
                // if (enumerator.MoveNext) goto loopStart;
                Emit(loopCondition);
                EmitLocalVariable(enumeratorVariable, EmissionType.ThisArg);
                Call(enumeratorVariable.VariableType, moveNextMethod);
                Brtrue(loopStart);

                if (shouldAttemptToDispose)
                {
                    Leave(endFinally);
                    Emit(endTry);
                }
            }
            // finally
            {
                if (shouldAttemptToDispose)
                {
                    var justBeforeEndFinally = CreateLabel();

                    var disposeMethod = AssemblyRegistry.GetMethod(Assembly, idisposable, "Dispose");
                    VariableDefinition disposableVariable = null;

                    // If enumerator is not disposable, we have to check it at runtime
                    if (!isDisposable)
                    {
                        disposableVariable = temporaryVariables.Acquire(idisposable);

                        // Check if enumerator is disposable, and if it is, store it into a temporary variable
                        // Otherwise jump to just before finally end
                        EmitLocalVariable(enumeratorVariable, EmissionType.Value);
                        EmitConversionIfNeeded(enumeratorVariable.VariableType, Assembly.TypeSystem.Object);
                        Isinst(idisposable);
                        Stloc(disposableVariable.Index);
                        EmitLocalVariable(disposableVariable, EmissionType.Value);
                        Brfalse(justBeforeEndFinally);

                        // Dispose enumerator
                        EmitLocalVariable(disposableVariable, EmissionType.ThisArg);
                        Call(disposableVariable.VariableType, disposeMethod);
                    }
                    else
                    {
                        // Check if enumerator is null, jump to just before finally end

                        if (!enumeratorType.IsValueType)
                        {
                            EmitLocalVariable(enumeratorVariable, EmissionType.Value);
                            Brfalse(justBeforeEndFinally);
                        }

                        EmitLocalVariable(enumeratorVariable, EmissionType.ThisArg);
                        Call(enumeratorVariable.VariableType, AssemblyRegistry.GetCompatibleMethod(Assembly, enumeratorType, "Dispose", new TypeReference[0]));
                    }

                    // Done
                    Emit(justBeforeEndFinally);
                    EndFinally();
                    Emit(endFinally);

                    body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
                    {
                        TryStart = beginTry,
                        TryEnd = endTry,
                        HandlerStart = endTry,
                        HandlerEnd = endFinally
                    });
                }
            }

            temporaryVariables.Release(enumeratorVariable);
        }

        #endregion

        #region Expression node

        [Pure]
        private bool NeedsStorePrologue(IExpressionNode expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.ArrayAccess:
                case ExpressionNodeType.Field:
                case ExpressionNodeType.IndexOperator:
                case ExpressionNodeType.Property:
                    return true;

                case ExpressionNodeType.IncrementDecrementOperator:
                    return NeedsStorePrologue(((IIncrementDecrementOperatorNode)expression).Operand);
            }

            return false;
        }

        private void EmitStorePrologue(IExpressionNode expression)
        {
            Contract.Requires(NeedsStorePrologue(expression));

            switch (expression.ExpressionType)
            {
                case ExpressionNodeType.ArrayAccess:
                    EmitStorePrologue((IArrayAccessNode)expression);
                    break;

                case ExpressionNodeType.Field:
                    EmitStorePrologue((IFieldNode)expression);
                    return;

                case ExpressionNodeType.IncrementDecrementOperator:
                    EmitStorePrologue((IIncrementDecrementOperatorNode)expression);
                    return;

                case ExpressionNodeType.IndexOperator:
                    EmitStorePrologue((IIndexOperatorNode)expression);
                    return;

                case ExpressionNodeType.Property:
                    EmitStorePrologue((IPropertyNode)expression);
                    return;

                default:
                    ContractsHelper.AssumeUnreachable(string.Format("{0} ExpressionNode does not support emitting store prologue.", expression.ExpressionType));
                    return;
            }
        }

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

                case ExpressionNodeType.IncrementDecrementOperator:
                    EmitStore((IIncrementDecrementOperatorNode)expression);
                    return;

                case ExpressionNodeType.IndexOperator:
                    EmitStore((IIndexOperatorNode)expression);
                    return;

                case ExpressionNodeType.LocalVariable:
                    EmitStore((ILocalVariableNode)expression);
                    return;

                case ExpressionNodeType.Property:
                    EmitStore((IPropertyNode)expression);
                    return;

                default:
                    ContractsHelper.AssumeUnreachable(string.Format("Cannot store {0} ExpressionNode.", expression.ExpressionType));
                    return;
            }
        }

        #region Load expression node

        private void Emit(IFieldNode field, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            if (!field.Field.Resolve().IsStatic)
            {
                Contract.Assume(field.ObjectInstance != null);
                Emit(field.ObjectInstance, EmissionType.ThisArg);

                if (ShouldEmitAddress(field, emissionType))
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
                var resolvedField = field.Field.Resolve();

                if (resolvedField.IsLiteral)
                {
                    var value = resolvedField.Constant;
                    EmitConstant(value);

                    if (ShouldEmitAddress(field, emissionType))
                        LoadAddressOfValue(field);
                }
                else
                {
                    if (ShouldEmitAddress(field, emissionType))
                    {
                        Ldsflda(field.Field);
                    }
                    else
                    {
                        Ldsfld(field.Field);
                    }
                }
            }
        }

        private void Emit(IParameterNode argument, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            Contract.Assume(argument.Parameter.Index >= 0);

            var index = argument.Parameter.Index + (methodDefinition.HasThis ? 1 : 0);

            if (ShouldEmitAddress(argument, emissionType))
            {
                Ldarga(index);
            }
            else
            {
                Ldarg(index);
            }
        }

        private void Emit(ILocalVariableNode variable, EmissionType emissionType)
        {
            EmitLocalVariable(variable.LocalVariable, emissionType);
        }

        private void EmitLocalVariable(VariableDefinition variable, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            if (ShouldEmitAddress(variable, emissionType))
            {
                Ldloca(variable.Index);
            }
            else
            {
                Ldloc(variable.Index);
            }
        }

        private void Emit(IPropertyNode property, EmissionType emissionType)
        {
            var getter = AssemblyRegistry.GetPropertyGetter(Assembly, property.Property);

            if (getter.HasThis)
            {
                Contract.Assume(property.ObjectInstance != null);
                Emit(property.ObjectInstance, EmissionType.ThisArg);
                Call(property.ObjectInstance.ExpressionReturnType, getter);
            }
            else
            {
                Call(null, getter);
            }


            if (emissionType == EmissionType.None)
            {
                Contract.Assume(property.ExpressionReturnType.MetadataType != MetadataType.Void);
                Pop();
            }
            else if (ShouldEmitAddress(property, emissionType))
            {
                LoadAddressOfValue(property);
            }
        }

        private void Emit(IArrayAccessNode arrayAccess, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            var array = arrayAccess.ObjectInstance;
            Contract.Assume(array != null && array.ExpressionReturnType is ArrayType);

            var arrayType = (ArrayType)array.ExpressionReturnType;
            var indices = arrayAccess.Indices;

            Emit(array, EmissionType.ThisArg);

            if (arrayType.IsVector)
            {
                Contract.Assume(indices.Count == 1);
                EmitExpressionWithTargetType(indices[0], Assembly.TypeSystem.Int32);

                if (ShouldEmitAddress(arrayAccess, emissionType))
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
                var loadElementMethod = ShouldEmitAddress(arrayAccess, emissionType) ? AssemblyRegistry.GetArrayLoadElementAddress(arrayType) : AssemblyRegistry.GetArrayLoadElement(arrayType);
                EmitArgumentsForCall(indices, loadElementMethod);
                Call(array.ExpressionReturnType, loadElementMethod);
            }
        }

        private void Emit(IIndexOperatorNode indexOperator, EmissionType emissionType)
        {
            var getter = AssemblyRegistry.GetPropertyGetter(Assembly, indexOperator.Property);
            Contract.Assume(getter != null);

            if (getter.HasThis)
            {
                Contract.Assume(indexOperator.ObjectInstance != null);
                Emit(indexOperator.ObjectInstance, EmissionType.ThisArg);
                EmitArgumentsForCall(indexOperator.Indices, getter);
                Call(indexOperator.ObjectInstance.ExpressionReturnType, getter);
            }
            else
            {
                EmitArgumentsForCall(indexOperator.Indices, getter);
                Call(null, getter);
            }

            if (emissionType == EmissionType.None)
            {
                Contract.Assume(indexOperator.ExpressionReturnType.MetadataType != MetadataType.Void);
                Pop();
            }
            else if (ShouldEmitAddress(indexOperator, emissionType))
            {
                LoadAddressOfValue(indexOperator);
            }
        }

        #endregion

        #region Assignment

        private void Emit(IAssignmentOperatorNode assignmentOperator, EmissionType emissionType)
        {
            switch (assignmentOperator.RightOperand.ExpressionType)
            {
                case ExpressionNodeType.ValueCreation:
                    EmitValueCreationAssignment(assignmentOperator, emissionType);
                    return;
                    
                default:
                    EmitAssignment(assignmentOperator, emissionType);
                    return;
            }
        }

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
        // If storing left side requires prologue in stack before the value (for example, emitting 'this' pointer) OR we need to load address to the value:
        //  - Emit storing prologue (if needed)
        //  - Emit right side
        //  - Dup (*)
        //  - Stloc temp (*)
        //  - EmitStore left side
        //  - Ldloc(a) temp (*) 
        // Else:
        //  - Emit right side
        //  - Dup (*)
        //  - EmitStore left side
        // In case of duplicating left side if it's a property, we don't actually call a getter on it,
        // because that would be inefficient and could change semantics if the getter doesn't return the same value
        private void EmitAssignment(IAssignmentOperatorNode assignmentOperator, EmissionType emissionType)
        {
            // If we're storing to field or property and it's not static, we need to load object instance now
            // and if we're also duplicating value, we got to save it to temp variable
            VariableDefinition tempVariable = null;

            bool needsStorePrologue = NeedsStorePrologue(assignmentOperator.LeftOperand);
            bool shouldEmitAddress = ShouldEmitAddress(assignmentOperator, emissionType);
            bool storeResultInTempVariable = needsStorePrologue || shouldEmitAddress;

            if (needsStorePrologue)
            {
                EmitStorePrologue(assignmentOperator.LeftOperand);
            }

            EmitExpressionWithTargetType(assignmentOperator.RightOperand, assignmentOperator.LeftOperand.ExpressionReturnType);

            if (emissionType != EmissionType.None)
            {
                Dup();

                if (storeResultInTempVariable)
                {
                    // Right operand could be a different type, 
                    // but it will get casted to left operand type
                    tempVariable = temporaryVariables.Acquire(assignmentOperator.LeftOperand.ExpressionReturnType);
                    Stloc(tempVariable.Index);
                }
            }

            EmitStore(assignmentOperator.LeftOperand);

            if (emissionType != EmissionType.None && storeResultInTempVariable)
            {
                if (shouldEmitAddress)
                {
                    Ldloca(tempVariable.Index);
                }
                else
                {
                    Ldloc(tempVariable.Index);
                    temporaryVariables.Release(tempVariable);
                }
            }
        }

        private void EmitValueCreationAssignment(IAssignmentOperatorNode assignmentOperator, EmissionType emissionType)
        {
            var leftOperand = assignmentOperator.LeftOperand;
            bool canInitObjDirectly = leftOperand.ExpressionReturnType == assignmentOperator.RightOperand.ExpressionReturnType;

            if (canInitObjDirectly)
            {
                switch (leftOperand.ExpressionType)
                {
                    case ExpressionNodeType.Field:
                    case ExpressionNodeType.FunctionArgument:
                    case ExpressionNodeType.LocalVariable:
                        break;

                    default:
                        canInitObjDirectly = false;
                        break;
                }
            }

            if (canInitObjDirectly)
            {
                if (!ShouldEmitAddress(assignmentOperator, emissionType))
                {
                    Emit(leftOperand, EmissionType.ReferenceToValue);
                    Initobj(leftOperand.ExpressionReturnType);

                    if (emissionType != EmissionType.None)
                        Emit(leftOperand, EmissionType.Value);
                }
                else
                {
                    Emit(leftOperand, EmissionType.ReferenceToValue);
                    Dup();
                    Initobj(leftOperand.ExpressionReturnType);
                }
            }
            else
            {
                EmitAssignment(assignmentOperator, emissionType);
            }
        }

        private void EmitExpressionWithTargetType(IExpressionNode expression, TypeReference targetType, bool forcePrimitiveConversion = false)
        {
            if (targetType is ByReferenceType)
            {
                if (expression.ExpressionType == ExpressionNodeType.Null)
                {
                    EmitReferenceToNull(targetType);
                    return;
                }

                Emit(expression, EmissionType.ReferenceToValue);
                return;
            }

            bool expressionIsFunction = expression.ExpressionType == ExpressionNodeType.Function;
            bool expressionIsFunctor = expression.ExpressionReturnType.IsFunctorType();

            bool targetIsDelegate = targetType.IsDelegate();

            if (targetIsDelegate && expressionIsFunction)
            {
                EmitMethodNodeAsDelegate((IMethodNode)expression, targetType);
                return;
            }

            // We'll want to emit expression in all following cases
            Emit(expression, EmissionType.Value);

            if (targetIsDelegate && expressionIsFunctor)
            {
                // Here we have a functor object on top of the stack

                var delegateType = targetType;
                var functorType = expression.ExpressionReturnType;

                var asDelegateMethod = AssemblyRegistry.GetMethod(Assembly, functorType, "AsDelegate");
                var delegateInvokeMethod = AssemblyRegistry.GetMethod(Assembly, asDelegateMethod.GetReturnType(), "Invoke");
                var delegateCtor = AssemblyRegistry.GetMethod(Assembly, delegateType, ".ctor");

                Callvirt(asDelegateMethod);
                Ldftn(delegateInvokeMethod);
                Newobj(delegateCtor);
            }
            else
            {
                EmitConversionIfNeeded(expression.ExpressionReturnType, targetType, forcePrimitiveConversion);
            }
        }

        #endregion

        private void Emit(IBinaryOperatorNode binaryOperator, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            EmitBinaryOperator(binaryOperator, emissionType);

            if (ShouldEmitAddress(binaryOperator, emissionType))
                LoadAddressOfValue(binaryOperator);
        }

        private void EmitBinaryOperator(IBinaryOperatorNode binaryOperator, EmissionType emissionType)
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

        private void EmitMethodNodeAsDelegate(IMethodNode function, TypeReference delegateType)
        {
            var ctor = AssemblyRegistry.GetMethod(Assembly, delegateType, ".ctor");

            if (function.Method.HasThis)
            {
                Contract.Assume(function.ObjectInstance != null);
                EmitExpressionWithTargetType(function.ObjectInstance, Assembly.TypeSystem.Object);
            }
            else
            {
                Ldnull();
            }

            Ldftn(function.Method);
            Newobj(ctor);
        }

        private void Emit(IMethodNode function, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            var returnTypeIsDelegate = function.ExpressionReturnType.IsDelegate();

            if (!returnTypeIsDelegate)
            {
                var functorType = AssemblyRegistry.GetImplementationFunctorType(Assembly, DeclaringType, function.Method);
                var ctor = AssemblyRegistry.GetMethod(Assembly, functorType, ".ctor");

                if (function.Method.HasThis)
                {
                    Contract.Assume(function.ObjectInstance != null);
                    Emit(function.ObjectInstance, EmissionType.ThisArg);
                }

                Newobj(ctor);
                if (ShouldEmitAddress(function, emissionType))
                    LoadAddressOfValue(function);
            }
            else
            {
                EmitMethodNodeAsDelegate(function, function.ExpressionReturnType);
            }

            if (ShouldEmitAddress(function, emissionType))
                LoadAddressOfValue(function);
        }

        private void Emit(IFunctionCallNode functionCall, EmissionType emissionType)
        {
            var function = functionCall.Function;

            if (function.ExpressionType == ExpressionNodeType.Function)
            {
                // Direct call
                var functionNode = (IMethodNode)function;

                if (functionNode.Method.HasThis)
                {
                    Contract.Assume(functionNode.ObjectInstance != null);
                    Emit(functionNode.ObjectInstance, EmissionType.ThisArg);

                    EmitArgumentsForCall(functionCall.Args, functionNode.Method);
                    Call(functionNode.ObjectInstance.ExpressionReturnType, functionNode.Method);
                }
                else
                {
                    EmitArgumentsForCall(functionCall.Args, functionNode.Method);
                    Call(null, functionNode.Method);
                }
            }
            else
            {
                // Functor Call
                var invokeMethod = AssemblyRegistry.GetMethod(Assembly, function.ExpressionReturnType, "Invoke");

                Emit(function, EmissionType.ThisArg);
                EmitArgumentsForCall(functionCall.Args, invokeMethod);

                Callvirt(invokeMethod);
            }

            if (emissionType == EmissionType.None)
            {
                if (functionCall.ExpressionReturnType.MetadataType != MetadataType.Void)
                    Pop();
            }
            else if (ShouldEmitAddress(functionCall, emissionType))
            {
                LoadAddressOfValue(functionCall);
            }
        }

        private void EmitArgumentsForCall(IReadOnlyList<IExpressionNode> arguments, MethodReference method)
        {
            var methodParameters = method.GetParameterTypes();
            var resolvedMethod = method.Resolve();

            bool isParamsCall = resolvedMethod != null && resolvedMethod.IsParamsMethod() && arguments.Count >= methodParameters.Count;

            if (isParamsCall && arguments.Count == methodParameters.Count)
            {
                var lastParameterIndex = methodParameters.Count - 1;
                isParamsCall &= !arguments[lastParameterIndex].ExpressionReturnType.IsAssignableTo(methodParameters[lastParameterIndex]);
            }

            if (isParamsCall)
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
                    EmitArgumentForCall(arguments[i], methodParameters[i]);
                }
            }
        }

        private void EmitArgumentsForParamsCall(IReadOnlyList<IExpressionNode> arguments, IReadOnlyList<TypeReference> methodParameterTypes)
        {
            for (int i = 0; i < methodParameterTypes.Count - 1; i++)
            {
                EmitArgumentForCall(arguments[i], methodParameterTypes[i]);
            }

            var arrayVariable = temporaryVariables.Acquire(methodParameterTypes[methodParameterTypes.Count - 1]);
            var elementType = methodParameterTypes[methodParameterTypes.Count - 1].GetElementType();

            Ldc_I4(arguments.Count - methodParameterTypes.Count + 1);
            Newarr(elementType);
            Stloc(arrayVariable.Index);

            for (int i = methodParameterTypes.Count - 1; i < arguments.Count; i++)
            {
                Ldloc(arrayVariable.Index);
                Ldc_I4(i - methodParameterTypes.Count + 1);
                EmitArgumentForCall(arguments[i], elementType);
                Stelem(elementType);
            }

            Ldloc(arrayVariable.Index);
            temporaryVariables.Release(arrayVariable);
        }

        private void EmitArgumentsForCallWithOptionalParameters(IReadOnlyList<IExpressionNode> arguments, IReadOnlyList<TypeReference> methodParameterTypes, MethodDefinition resolvedMethod)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                EmitArgumentForCall(arguments[i], methodParameterTypes[i]);
            }

            for (int i = arguments.Count; i < methodParameterTypes.Count; i++)
            {
                // Use resolved method here for getting constant value, as it will not be present in method reference parameters.
                // Furthermore, we must not reference resolved method parameter TYPES as they will be resolved as well
                var defaultValue = resolvedMethod.Parameters[i].Constant;

                if (defaultValue == null)
                {
                    Ldnull();
                    continue;
                }

                EmitConstant(defaultValue);
            }
        }

        private void EmitConstant(object value)
        {
            var constantType = value.GetType();

            if (constantType == typeof(SByte))
            {
                Ldc_I4((SByte)value);
            }
            else if (constantType == typeof(Byte))
            {
                Ldc_I4((Byte)value);
            }
            else if (constantType == typeof(Int16))
            {
                Ldc_I4((Int16)value);
            }
            else if (constantType == typeof(UInt16))
            {
                Ldc_I4((UInt16)value);
            }
            else if (constantType == typeof(Char))
            {
                Ldc_I4((Char)value);
            }
            else if (constantType == typeof(Int32))
            {
                Ldc_I4((Int32)value);
            }
            else if (constantType == typeof(UInt32))
            {
                Ldc_I4((int)(UInt32)value);
            }
            else if (constantType == typeof(Int64))
            {
                Ldc_I8((Int64)value);
            }
            else if (constantType == typeof(UInt64))
            {
                Ldc_I8((long)(UInt64)value);
            }
            else if (constantType == typeof(float))
            {
                Ldc_R4((float)value);
            }
            else if (constantType == typeof(double))
            {
                Ldc_R8((double)value);
            }
            else if (constantType == typeof(string))
            {
                Ldstr((string)value);
            }
            else
            {
                ContractsHelper.AssumeUnreachable(string.Format("Unknown default value literal: {0} with value of {1}.",
                    value.GetType().FullName, value));
            }
        }

        private void EmitArgumentForCall(IExpressionNode argument, TypeReference targetParameterType)
        {
            EmitExpressionWithTargetType(argument, targetParameterType);
        }

        private void Emit(ILiteralNode literal, EmissionType emissionType)
        {
            if (emissionType == EmissionType.None)
                return;

            switch (literal.ExpressionReturnType.MetadataType)
            {
                case MetadataType.Boolean:
                    Ldc_I4((bool)literal.Value ? 1 : 0);
                    break;

                case MetadataType.SByte:
                    Ldc_I4((sbyte)literal.Value);
                    break;

                case MetadataType.Int16:
                    Ldc_I4((short)literal.Value);
                    break;

                case MetadataType.Int32:
                    Ldc_I4((int)literal.Value);
                    break;

                case MetadataType.Int64:
                    Ldc_I8((long)literal.Value);
                    break;

                case MetadataType.Byte:
                    Ldc_I4((byte)literal.Value);
                    break;

                case MetadataType.UInt16:
                    Ldc_I4((ushort)literal.Value);
                    break;

                case MetadataType.Char:
                    Ldc_I4((char)literal.Value);
                    break;

                // first cast is checked
                case MetadataType.UInt32:
                    Ldc_I4((int)(uint)literal.Value);
                    break;

                // first cast is checked
                case MetadataType.UInt64:
                    Ldc_I8((long)(ulong)literal.Value);
                    break;

                case MetadataType.Single:
                    Ldc_R4((float)literal.Value);
                    break;

                case MetadataType.Double:
                    Ldc_R8((double)literal.Value);
                    break;

                case MetadataType.String:
                    Ldstr((string)literal.Value);
                    break;

                default:
                    ContractsHelper.AssertUnreachable("Unknown literal type: " + literal.ExpressionReturnType.FullName);
                    return;
            }

            if (ShouldEmitAddress(literal, emissionType))
                LoadAddressOfValue(literal);
        }

        private void EmitValueCreation(IExpressionNode valueCreation, EmissionType emissionType)
        {
            Contract.Requires(valueCreation.ExpressionReturnType.IsValueType);

            if (emissionType == EmissionType.None)
                return;

            var variableToInitialize = temporaryVariables.Acquire(valueCreation.ExpressionReturnType);
            
            if (!ShouldEmitAddress(valueCreation, emissionType))
            {
                Ldloca(variableToInitialize.Index);
                Initobj(valueCreation.ExpressionReturnType);
                Ldloc(variableToInitialize.Index);
                temporaryVariables.Release(variableToInitialize);
            }
            else
            {
                Ldloca(variableToInitialize.Index);
                Dup();
                Initobj(valueCreation.ExpressionReturnType);
            }
        }

        private void Emit(IObjectCreationNode objectCreation, EmissionType emissionType)
        {
            EmitArgumentsForCall(objectCreation.Args, objectCreation.Constructor);
            Newobj(objectCreation.Constructor);

            if (emissionType == EmissionType.None)
            {
                Contract.Assume(objectCreation.ExpressionReturnType.MetadataType != MetadataType.Void);
                Pop();
            }
            else if (ShouldEmitAddress(objectCreation, emissionType))
            {
                LoadAddressOfValue(objectCreation);
            }
        }

        private void Emit(IArrayCreationNode arrayCreation, EmissionType emissionType)
        {
            Contract.Requires(arrayCreation.ExpressionReturnType.IsArray, "Return type of IArrayCreationNode must be an array type.");
            Contract.Requires(arrayCreation.Dimensions.Count == ((ArrayType)arrayCreation.ExpressionReturnType).Rank, "Array creation node dimension count must match array type rank.");

            if (emissionType == EmissionType.None)
                return;

            var arrayType = (ArrayType)arrayCreation.ExpressionReturnType;

            if (arrayType.IsVector)
            {
                EmitExpressionWithTargetType(arrayCreation.Dimensions[0], Assembly.TypeSystem.Int32);
                Newarr(arrayType.ElementType);
            }
            else
            {
                var rank = arrayType.Rank;
                var constructor = AssemblyRegistry.GetArrayConstructor(arrayType);

                for (int i = 0; i < rank; i++)
                {
                    EmitExpressionWithTargetType(arrayCreation.Dimensions[i], Assembly.TypeSystem.Int32);
                }

                Newobj(constructor);
            }

            if (arrayCreation.Initializer != null)
            {
                if (CanEmitArrayInitializerFastPath(arrayType.ElementType, arrayCreation.Initializer))
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

            if (ShouldEmitAddress(arrayCreation, emissionType))
                LoadAddressOfValue(arrayCreation);
        }

        [Pure]
        private bool CanEmitArrayInitializerFastPath(TypeReference elementType, IReadOnlyList<IExpressionNode> initializer)
        {
            if (!elementType.IsValueType)
                return false;

            for (int i = 0; i < initializer.Count; i++)
            {
                if (initializer[i].ExpressionType != ExpressionNodeType.Literal || !initializer[i].ExpressionReturnType.IsValueType)
                    return false;
            }

            return initializer.Count > 2;   // It's not worth calling initializer function if there's less than 3 elements in the array
        }

        // Assumes array is on the stack but it must leave it on the stack after the function is done
        private void EmitInitializerFastPath(ArrayType arrayType, IReadOnlyList<IExpressionNode> initializer)
        {
            Contract.Requires(CanEmitArrayInitializerFastPath(arrayType.ElementType, initializer));

            var field = AssemblyRegistry.GetArrayInitializerField(Assembly, arrayType.ElementType, initializer);
            var initializeArrayMethod = AssemblyRegistry.GetMethod(Assembly, "System.Runtime.CompilerServices.RuntimeHelpers", "InitializeArray");

            Dup();
            Ldtoken(field);
            Call(arrayType, initializeArrayMethod);
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
                EmitExpressionWithTargetType(initializer[i], arrayType.ElementType);
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

                EmitExpressionWithTargetType(initializer[i], arrayType.ElementType);
                Call(arrayType, storeElementMethod);
            }

            CurrentSequencePoint = oldSequencePoint;
        }

        private void EmitThis(IExpressionNode expression, EmissionType emissionType)
        {
            Contract.Requires(!methodDefinition.IsStatic);

            if (emissionType == EmissionType.None)
                return;

            Ldarg(0);

            if (emissionType == EmissionType.ReferenceToValue && !methodDefinition.DeclaringType.IsValueType)
                LoadAddressOfValue(expression);
        }

        private void EmitNull(IExpressionNode expression, EmissionType emissionType)
        {
            Contract.Requires(emissionType != EmissionType.ReferenceToValue);

            if (emissionType != EmissionType.None)
                Ldnull();
        }

        private void EmitReferenceToNull(TypeReference type)
        {
            Contract.Requires(!type.IsValueType);
            var temporaryVariable = temporaryVariables.Acquire(type);
            
            Ldnull();
            Stloc(temporaryVariable.Index);
            Ldloca(temporaryVariable.Index);
        }

        private void Emit(IUnaryOperatorNode unaryOperator, EmissionType emissionType)
        {
            
            if (emissionType == EmissionType.None)
                return;

            Emit(unaryOperator.Operand, EmissionType.Value);

            switch (unaryOperator.UnaryOperatorType)
            {
                case UnaryOperatorNodeType.BinaryNot:
                    Not();
                    break;

                case UnaryOperatorNodeType.LogicalNot:
                    Ldc_I4(0);
                    Ceq();
                    break;

                case UnaryOperatorNodeType.Negation:
                    Neg();
                    break;

                default:
                    ContractsHelper.AssertUnreachable(string.Format("Unknown unary operator type: {0}", unaryOperator.UnaryOperatorType));
                    return;
            }

            if (ShouldEmitAddress(unaryOperator, emissionType))
                LoadAddressOfValue(unaryOperator);
        }

        private void EmitIncrementDecrement(IIncrementDecrementOperatorNode incrementDecrementOperator, EmissionType emissionType, Action beforeValueSnapshot, Action afterValueSnapshot)
        {
            VariableDefinition tempVariable;
            var operand = incrementDecrementOperator.Operand;
            bool hasStoreEpilogue = NeedsStorePrologue(operand);

            if (hasStoreEpilogue)
                EmitStorePrologue(operand);

            Emit(operand, EmissionType.Value);
            beforeValueSnapshot();

            if (emissionType != EmissionType.None)
                Dup();

            if (emissionType != EmissionType.None && hasStoreEpilogue)
            {
                tempVariable = temporaryVariables.Acquire(operand.ExpressionReturnType);
                Stloc(tempVariable.Index);
                afterValueSnapshot();
                Ldloc(tempVariable.Index);
            }
            else
            {
                afterValueSnapshot();
            }

            if (ShouldEmitAddress(incrementDecrementOperator, emissionType))
                LoadAddressOfValue(incrementDecrementOperator);
        }

        private void Emit(IIncrementDecrementOperatorNode incrementDecrementOperator, EmissionType emissionType)
        {
            Action beforeValueSnapshot, afterValueSnapshot;

            if (incrementDecrementOperator.OverloadedOperatorMethod != null)
            {
                switch (incrementDecrementOperator.IncrementDecrementType)
                {
                    case IncrementDecrementOperatorType.PreDecrement:
                    case IncrementDecrementOperatorType.PreIncrement:
                        beforeValueSnapshot = () => Call(null, incrementDecrementOperator.OverloadedOperatorMethod);
                        afterValueSnapshot = () => EmitStore(incrementDecrementOperator.Operand);
                        break;

                    case IncrementDecrementOperatorType.PostDecrement:
                    case IncrementDecrementOperatorType.PostIncrement:
                        beforeValueSnapshot = () => { };
                        afterValueSnapshot = () =>
                        {
                            Call(null, incrementDecrementOperator.OverloadedOperatorMethod);
                            EmitStore(incrementDecrementOperator.Operand);
                        };
                        break;

                    default:
                        ContractsHelper.AssertUnreachable(string.Format("Unknown unary operator type: {0}", incrementDecrementOperator.IncrementDecrementType));
                        return;
                }
            }
            else
            {
                switch (incrementDecrementOperator.IncrementDecrementType)
                {
                    case IncrementDecrementOperatorType.PreDecrement:
                        beforeValueSnapshot = () =>
                        {
                            Ldc_I4(-1);
                            Add();
                        };
                        afterValueSnapshot = () => EmitStore(incrementDecrementOperator.Operand);
                        break;

                    case IncrementDecrementOperatorType.PreIncrement:
                        beforeValueSnapshot = () =>
                        {
                            Ldc_I4(1);
                            Add();
                        };
                        afterValueSnapshot = () => EmitStore(incrementDecrementOperator.Operand);
                        break;

                    case IncrementDecrementOperatorType.PostDecrement:
                        beforeValueSnapshot = () => { };
                        afterValueSnapshot = () =>
                        {
                            Ldc_I4(-1);
                            Add();
                            EmitStore(incrementDecrementOperator.Operand);
                        };
                        break;

                    case IncrementDecrementOperatorType.PostIncrement:
                        beforeValueSnapshot = () => { };
                        afterValueSnapshot = () =>
                        {
                            Ldc_I4(1);
                            Add();
                            EmitStore(incrementDecrementOperator.Operand);
                        };
                        break;

                    default:
                        ContractsHelper.AssertUnreachable(string.Format("Unknown unary operator type: {0}", incrementDecrementOperator.IncrementDecrementType));
                        return;
                }
            }

            EmitIncrementDecrement(incrementDecrementOperator, emissionType, beforeValueSnapshot, afterValueSnapshot);
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
            Emit(left, EmissionType.Value);

            if (left.ExpressionReturnType.IsValueType)
            {
                Box(left.ExpressionReturnType);
            }

            Emit(right, EmissionType.Value);

            if (right.ExpressionReturnType.IsValueType)
            {
                Box(right.ExpressionReturnType);
            }

            var concatMethod = AssemblyRegistry.GetCompatibleMethod(Assembly, Assembly.TypeSystem.String, "Concat", new TypeReference[]
                {
                    Assembly.TypeSystem.Object,
                    Assembly.TypeSystem.Object
                });

            Call(null, concatMethod);
        }

        #endregion

        #region Logical And/Logical Or emitters

        private void EmitLogicalAnd(IBinaryOperatorNode binaryOperator)
        {
            var emitFalse = ilProcessor.Create(OpCodes.Ldc_I4, 0);
            var end = CreateLabel();

            Emit(binaryOperator.LeftOperand, EmissionType.Value);
            Brfalse(emitFalse);

            Emit(binaryOperator.RightOperand, EmissionType.Value);
            Br(end);

            Emit(emitFalse);
            Emit(end);
        }

        private void EmitLogicalOr(IBinaryOperatorNode binaryOperator)
        {
            var emitTrue = ilProcessor.Create(OpCodes.Ldc_I4, 1);
            var end = CreateLabel();

            Emit(binaryOperator.LeftOperand, EmissionType.Value);
            Brtrue(emitTrue);

            Emit(binaryOperator.RightOperand, EmissionType.Value);
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

            Emit(left, EmissionType.Value);
            Emit(right, EmissionType.Value);

            Call(null, stringComparisonMethod);

            Ldc_I4(-1);
            Cgt();
        }

        private void EmitGreaterEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            if (left.ExpressionReturnType.IsUnsignedInteger() && right.ExpressionReturnType.IsUnsignedInteger())
            {
                Clt_Un();
            }
            else
            {
                Clt();
            }

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


            Emit(left, EmissionType.Value);
            Emit(right, EmissionType.Value);

            Call(null, stringComparisonMethod);

            Ldc_I4(0);
            Cgt();
        }

        private void EmitGreaterThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            if (left.ExpressionReturnType.IsUnsignedInteger() && right.ExpressionReturnType.IsUnsignedInteger())
            {
                Cgt_Un();
            }
            else
            {
                Cgt();
            }
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


            Emit(left, EmissionType.Value);
            Emit(right, EmissionType.Value);

            Call(null, stringComparisonMethod);

            Ldc_I4(1);
            Clt();
        }

        private void EmitLessEqualThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            if (left.ExpressionReturnType.IsUnsignedInteger() && right.ExpressionReturnType.IsUnsignedInteger())
            {
                Cgt_Un();
            }
            else
            {
                Cgt();
            }

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


            Emit(left, EmissionType.Value);
            Emit(right, EmissionType.Value);

            Call(null, stringComparisonMethod);

            Ldc_I4(0);
            Clt();
        }

        private void EmitLessThanNumeral(IExpressionNode left, IExpressionNode right)
        {
            EmitOperandsAndConvertIfNeeded(left, right);

            if (left.ExpressionReturnType.IsUnsignedInteger() && right.ExpressionReturnType.IsUnsignedInteger())
            {
                Clt_Un();
            }
            else
            {
                Clt();
            }
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

            Emit(binaryOperator.LeftOperand, EmissionType.Value);
            Emit(binaryOperator.RightOperand, EmissionType.Value);

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

        private void Emit(ICastNode castNode, EmissionType emissionType)
        {
            EmitExpressionWithTargetType(castNode.TargetExpression, castNode.ExpressionReturnType, true);

            if (ShouldEmitAddress(castNode, emissionType))
                LoadAddressOfValue(castNode);
        }

        #endregion

        #region Store expression node

        private void EmitStorePrologue(IArrayAccessNode arrayAccess)
        {
            Contract.Assume(arrayAccess.ObjectInstance != null);
            Emit(arrayAccess.ObjectInstance, EmissionType.ThisArg);

            var arrayType = (ArrayType)arrayAccess.ObjectInstance.ExpressionReturnType;
            var indices = arrayAccess.Indices;

            if (arrayType.IsVector)
            {
                Contract.Assume(indices.Count == 1);
                EmitExpressionWithTargetType(indices[0], Assembly.TypeSystem.Int32);
            }
            else
            {
                var storeElementMethod = AssemblyRegistry.GetArrayStoreElement(arrayType);
                var parameterTypes = storeElementMethod.GetParameterTypes();

                for (int i = 0; i < indices.Count; i++)
                {
                    EmitArgumentForCall(indices[i], parameterTypes[i]);
                }
            }
        }

        private void EmitStorePrologue(IFieldNode field)
        {
            if (!field.Field.Resolve().IsStatic)
            {
                Contract.Assume(field.ObjectInstance != null);
                Emit(field.ObjectInstance, EmissionType.ThisArg);
            }
        }

        private void EmitStorePrologue(IIncrementDecrementOperatorNode incrementDecrementOperator)
        {
            switch (incrementDecrementOperator.IncrementDecrementType)
            {
                case IncrementDecrementOperatorType.PreDecrement:
                case IncrementDecrementOperatorType.PreIncrement:
                    EmitStorePrologue(incrementDecrementOperator.Operand);
                    return;
            }

            ContractsHelper.AssertUnreachable(string.Format("Cannot store increment/decrement operator {0}.", incrementDecrementOperator.IncrementDecrementType));
        }

        private void EmitStorePrologue(IIndexOperatorNode indexOperator)
        {
            var setter = AssemblyRegistry.GetPropertySetter(Assembly, indexOperator.Property);
            Contract.Assume(setter != null);

            if (setter.HasThis)
            {
                Contract.Assume(indexOperator.ObjectInstance != null);
                Emit(indexOperator.ObjectInstance, EmissionType.ThisArg);
            }

            var parameterTypes = setter.GetParameterTypes();

            for (int i = 0; i < indexOperator.Indices.Count; i++)
            {
                EmitArgumentForCall(indexOperator.Indices[i], parameterTypes[i]);
            }
        }

        private void EmitStorePrologue(IPropertyNode property)
        {
            var propertyDef = property.Property.Resolve();
            Contract.Assume(propertyDef.SetMethod != null);

            if (propertyDef.SetMethod.HasThis)
            {
                Contract.Assume(property.ObjectInstance != null);
                Emit(property.ObjectInstance, EmissionType.ThisArg);
            }
        }

        private void EmitStore(IArrayAccessNode arrayAccess)
        {
            var array = arrayAccess.ObjectInstance;
            Contract.Assume(array != null && array.ExpressionReturnType is ArrayType);

            var arrayType = (ArrayType)array.ExpressionReturnType;

            if (arrayType.IsVector)
            {
                Stelem(arrayType.ElementType);
            }
            else
            {
                var storeElementMethod = AssemblyRegistry.GetArrayStoreElement(arrayType);
                Call(array.ExpressionReturnType, storeElementMethod);
            }
        }

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

        private void EmitStore(IIncrementDecrementOperatorNode incrementDecrementOperator)
        {
            switch (incrementDecrementOperator.IncrementDecrementType)
            {
                case IncrementDecrementOperatorType.PreDecrement:
                case IncrementDecrementOperatorType.PreIncrement:
                    EmitStore(incrementDecrementOperator.Operand);
                    return;
            }

            ContractsHelper.AssertUnreachable(string.Format("Cannot store increment/decrement operator {0}.", incrementDecrementOperator.IncrementDecrementType));
        }

        private void EmitStore(IIndexOperatorNode indexOperator)
        {
            var setter = AssemblyRegistry.GetPropertySetter(Assembly, indexOperator.Property);
            Contract.Assume(setter != null);

            var objectInstanceType = setter.HasThis ? indexOperator.ObjectInstance.ExpressionReturnType : null;
            Call(objectInstanceType, setter);
        }

        private void EmitStore(ILocalVariableNode variable)
        {
            Stloc(variable.LocalVariable.Index);
        }

        private void EmitStore(IParameterNode argument)
        {
            Starg(argument.Parameter.Index);
        }

        private void EmitStore(IPropertyNode property)
        {
            var setter = AssemblyRegistry.GetPropertySetter(Assembly, property.Property);
            var objectInstanceType = setter.HasThis ? property.ObjectInstance.ExpressionReturnType : null;
            Call(objectInstanceType, setter);
        }

        #endregion

        protected void EmitOperandsAndConvertIfNeeded(IExpressionNode left, IExpressionNode right)
        {
            TypeReference leftType = left.ExpressionReturnType;
            TypeReference rightType = right.ExpressionReturnType;

            bool conversionNeeded = leftType.FullName != rightType.FullName;

            if (!conversionNeeded)
            {
                Emit(left, EmissionType.Value);
                Emit(right, EmissionType.Value);
            }
            else if (leftType.IsAssignableTo(rightType))
            {
                Emit(left, EmissionType.Value);
                EmitConversionIfNeeded(leftType, rightType);
                Emit(right, EmissionType.Value);
            }
            else if (rightType.IsAssignableTo(leftType))
            {
                Emit(left, EmissionType.Value);
                Emit(right, EmissionType.Value);
                EmitConversionIfNeeded(rightType, leftType);
            }
            else
            {
                ContractsHelper.AssumeUnreachable(string.Format("{0} and {1} cannot be cast to each other!", leftType.FullName, rightType.FullName));
            }
        }

        protected void EmitConversionIfNeeded(TypeReference sourceType, TypeReference targetType, bool forcePrimitveConversion = false)
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
                Unbox_Any(targetType);
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
            var primitiveWidth = sourceType.GetPrimitiveWidth();

            switch (targetType.MetadataType)
            {
                case MetadataType.SByte:
                case MetadataType.Int16:
                case MetadataType.Int32:
                    {
                        if (sourceType.IsFloatingPointType() || primitiveWidth == 8 || forcePrimitveConversion)
                        {
                            Conv_I4();
                        }
                        else if (sourceType.IsUnsignedInteger() && primitiveWidth != 4)
                        {
                            Conv_U4();
                        }
                    }
                    break;

                case MetadataType.Byte:
                case MetadataType.Char:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                    {
                        if (sourceType.IsFloatingPointType() || primitiveWidth == 8 || forcePrimitveConversion)
                        {
                            Conv_U4();
                        }
                        else if (sourceType.IsSignedInteger() && primitiveWidth != 4)
                        {
                            Conv_I4();
                        }
                    }
                    break;

                case MetadataType.Int64:
                    if (sourceType.IsFloatingPointType() || primitiveWidth != 8 || forcePrimitveConversion)
                    {
                        if (sourceType.IsUnsignedInteger())
                        {
                            Conv_U8();
                        }
                        else
                        {
                            Conv_I8();
                        }
                    }
                    break;

                case MetadataType.UInt64:
                    if (sourceType.IsFloatingPointType() || primitiveWidth != 8 || forcePrimitveConversion)
                    {
                        if (sourceType.IsUnsignedInteger() || forcePrimitveConversion)
                        {
                            Conv_U8();
                        }
                        else
                        {
                            Conv_I8();
                        }
                    }
                    break;

                case MetadataType.IntPtr:
                    Conv_I();
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

        private bool ShouldEmitAddress(IExpressionNode expression, EmissionType emissionType)
        {
            return (expression.ExpressionReturnType.IsValueType && emissionType == EmissionType.ThisArg) || emissionType == EmissionType.ReferenceToValue;
        }

        private bool ShouldEmitAddress(VariableDefinition variable, EmissionType emissionType)
        {
            return (variable.VariableType.IsValueType && emissionType == EmissionType.ThisArg) || emissionType == EmissionType.ReferenceToValue;
        }

        private void LoadAddressOfValue(IExpressionNode expression)
        {
            var variable = temporaryVariables.Acquire(expression.ExpressionReturnType);
            Stloc(variable.Index);
            Ldloca(variable.Index);
        }

        #endregion
    }
}
