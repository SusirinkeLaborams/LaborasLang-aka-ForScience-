using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace LaborasLangCompiler.Codegen.Methods
{
    internal abstract class MethodEmitterBase
    {
        protected readonly MethodDefinition methodDefinition;
        protected readonly MethodBody body;
        protected readonly ILProcessor ilProcessor;
        protected TemporaryVariables temporaryVariables;    // Not readonly because struct

        protected TypeEmitter DeclaringType { get; private set; }
        protected AssemblyEmitter Assembly { get { return DeclaringType.Assembly; } }

        protected SequencePoint CurrentSequencePoint;

        public MethodEmitterBase(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes)
        {
            DeclaringType = declaringType;

            methodDefinition = new MethodDefinition(name, methodAttributes, returnType);
            declaringType.AddMethod(methodDefinition);

            body = methodDefinition.Body;
            ilProcessor = body.GetILProcessor();
            temporaryVariables = new TemporaryVariables(Assembly, body);

            Assembly.AddTypeUsage(returnType);
        }

        [Pure]
        public MethodReference Get()
        {
            return methodDefinition;
        }

        public ParameterDefinition AddArgument(TypeReference type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Invalid argument name: " + name != null ? name : "<null>");
            }

            var parameter = new ParameterDefinition(name, ParameterAttributes.None, type);
            methodDefinition.Parameters.Add(parameter);
            Assembly.AddTypeUsage(type);

            return parameter;
        }

        public ParameterDefinition AddArgument(ParameterDefinition parameter)
        {
            methodDefinition.Parameters.Add(parameter);
            Assembly.AddTypeUsage(parameter.ParameterType);

            return parameter;
        }

        protected ParameterDefinition AddArgument(TypeReference type)
        {
            var parameter = new ParameterDefinition(type);
            methodDefinition.Parameters.Add(parameter);
            Assembly.AddTypeUsage(parameter.ParameterType);

            return parameter;
        }

        protected void Emit(Instruction instruction)
        {
            body.Instructions.Add(instruction);
        }

        protected static Instruction CreateLabel()
        {
            return Instruction.Create(OpCodes.Nop);
        }

        protected static bool IsAtLeastOneOperandString(IBinaryOperatorNode binaryOperator)
        {
            var left = binaryOperator.LeftOperand;
            var right = binaryOperator.RightOperand;

            bool leftIsString = left.ExpressionReturnType.MetadataType == MetadataType.String;
            bool rightIsString = right.ExpressionReturnType.MetadataType == MetadataType.String;

            return leftIsString || rightIsString;
        }

        protected static bool AreBothOperandsUnsigned(IBinaryOperatorNode binaryOperator)
        {
            return binaryOperator.LeftOperand.ExpressionReturnType.IsUnsignedInteger() && binaryOperator.RightOperand.ExpressionReturnType.IsUnsignedInteger();
        }

        #region IL Instructions

        protected void Add()
        {
            ilProcessor.Emit(OpCodes.Add);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void And()
        {
            ilProcessor.Emit(OpCodes.And);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Bne_Un(Instruction targetInstruction)
        {
            ilProcessor.Emit(OpCodes.Bne_Un, targetInstruction);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Blt_Un(Instruction targetInstruction)
        {
            ilProcessor.Emit(OpCodes.Blt_Un, targetInstruction);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Box(TypeReference type)
        {
            Contract.Requires(type != null);

            ilProcessor.Emit(OpCodes.Box, type);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(type);
        }

        protected void Brfalse(Instruction target)
        {
            Contract.Requires(target != null);

            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Brfalse_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Brfalse, target);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Br(Instruction target)
        {
            Contract.Requires(target != null);

            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Br_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Br, target);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Brtrue(Instruction target)
        {
            Contract.Requires(target != null);

            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Brtrue_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Brtrue, target);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Call(TypeReference thisType, MethodReference method)
        {
            Contract.Requires(method != null);

            OpCode opcode = OpCodes.Call;

            if (method.HasThis && !method.DeclaringType.IsValueType)
            {
                var resolvedMethod = method.Resolve();

                if (resolvedMethod != null && resolvedMethod.IsVirtual)
                {
                    if (thisType.IsValueType)
                        ilProcessor.Emit(OpCodes.Constrained, thisType);

                    opcode = OpCodes.Callvirt;
                }
            }

            ilProcessor.Emit(opcode, method);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Callvirt(MethodReference method)
        {
            Contract.Requires(method != null);

            ilProcessor.Emit(OpCodes.Callvirt, method);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Castclass(TypeReference targetType)
        {
            Contract.Requires(targetType != null);

            ilProcessor.Emit(OpCodes.Castclass, targetType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(targetType);
        }

        protected void Ceq()
        {
            ilProcessor.Emit(OpCodes.Ceq);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Cgt()
        {
            ilProcessor.Emit(OpCodes.Cgt);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Cgt_Un()
        {
            ilProcessor.Emit(OpCodes.Cgt_Un);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Clt()
        {
            ilProcessor.Emit(OpCodes.Clt);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Clt_Un()
        {
            ilProcessor.Emit(OpCodes.Clt_Un);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_I()
        {
            ilProcessor.Emit(OpCodes.Conv_I);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_I1()
        {
            ilProcessor.Emit(OpCodes.Conv_I1);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_I2()
        {
            ilProcessor.Emit(OpCodes.Conv_I2);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_I4()
        {
            ilProcessor.Emit(OpCodes.Conv_I4);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_I8()
        {
            ilProcessor.Emit(OpCodes.Conv_I8);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_R_Un()
        {
            ilProcessor.Emit(OpCodes.Conv_R_Un);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_R4()
        {
            ilProcessor.Emit(OpCodes.Conv_R4);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_R8()
        {
            ilProcessor.Emit(OpCodes.Conv_R8);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_U()
        {
            ilProcessor.Emit(OpCodes.Conv_U);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_U1()
        {
            ilProcessor.Emit(OpCodes.Conv_U1);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_U2()
        {
            ilProcessor.Emit(OpCodes.Conv_U2);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_U4()
        {
            ilProcessor.Emit(OpCodes.Conv_U4);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Conv_U8()
        {
            ilProcessor.Emit(OpCodes.Conv_U8);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Div()
        {
            ilProcessor.Emit(OpCodes.Div);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Div_Un()
        {
            ilProcessor.Emit(OpCodes.Div_Un);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Dup()
        {
            ilProcessor.Emit(OpCodes.Dup);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void EndFinally()
        {
            ilProcessor.Emit(OpCodes.Endfinally);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Initobj(TypeReference type)
        {
            Contract.Requires(type != null);

            ilProcessor.Emit(OpCodes.Initobj, type);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(type);
        }

        protected void Isinst(TypeReference type)
        {
            Contract.Requires(type != null);

            ilProcessor.Emit(OpCodes.Isinst, type);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(type);
        }

        protected void Ldarg(int index)
        {
            if (index < 4)
            {
                switch (index)
                {
                    case 0:
                        ilProcessor.Emit(OpCodes.Ldarg_0);
                        break;

                    case 1:
                        ilProcessor.Emit(OpCodes.Ldarg_1);
                        break;

                    case 2:
                        ilProcessor.Emit(OpCodes.Ldarg_2);
                        break;

                    case 3:
                        ilProcessor.Emit(OpCodes.Ldarg_3);
                        break;
                }
            }
            else if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldarg_S, methodDefinition.Parameters[index]);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldarg, methodDefinition.Parameters[index]);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldarga(int index)
        {
            if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldarga_S, methodDefinition.Parameters[index]);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldarga, methodDefinition.Parameters[index]);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldc_I4(int value)
        {
            if (value > -2 && value < 9)
            {
                switch (value)
                {
                    case -1:
                        ilProcessor.Emit(OpCodes.Ldc_I4_M1);
                        break;

                    case 0:
                        ilProcessor.Emit(OpCodes.Ldc_I4_0);
                        break;

                    case 1:
                        ilProcessor.Emit(OpCodes.Ldc_I4_1);
                        break;

                    case 2:
                        ilProcessor.Emit(OpCodes.Ldc_I4_2);
                        break;

                    case 3:
                        ilProcessor.Emit(OpCodes.Ldc_I4_3);
                        break;

                    case 4:
                        ilProcessor.Emit(OpCodes.Ldc_I4_4);
                        break;

                    case 5:
                        ilProcessor.Emit(OpCodes.Ldc_I4_5);
                        break;

                    case 6:
                        ilProcessor.Emit(OpCodes.Ldc_I4_6);
                        break;

                    case 7:
                        ilProcessor.Emit(OpCodes.Ldc_I4_7);
                        break;

                    case 8:
                        ilProcessor.Emit(OpCodes.Ldc_I4_8);
                        break;
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

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldc_I8(long value)
        {
            ilProcessor.Emit(OpCodes.Ldc_I8, value);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldc_R4(float value)
        {
            ilProcessor.Emit(OpCodes.Ldc_R4, value);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldc_R8(double value)
        {
            ilProcessor.Emit(OpCodes.Ldc_R8, value);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldelem(TypeReference elementType)
        {
            Contract.Requires(elementType != null);

            switch (elementType.MetadataType)
            {
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                case MetadataType.Pointer:
                    ilProcessor.Emit(OpCodes.Ldelem_I);
                    break;

                case MetadataType.Boolean:
                case MetadataType.Byte:
                case MetadataType.SByte:
                    ilProcessor.Emit(OpCodes.Ldelem_I1);
                    break;

                case MetadataType.Char:
                case MetadataType.UInt16:
                case MetadataType.Int16:
                    ilProcessor.Emit(OpCodes.Ldelem_I2);
                    break;

                case MetadataType.UInt32:
                case MetadataType.Int32:
                    ilProcessor.Emit(OpCodes.Ldelem_I4);
                    break;

                case MetadataType.UInt64:
                case MetadataType.Int64:
                    ilProcessor.Emit(OpCodes.Ldelem_I8);
                    break;

                case MetadataType.Single:
                    ilProcessor.Emit(OpCodes.Ldelem_R4);
                    break;

                case MetadataType.Double:
                    ilProcessor.Emit(OpCodes.Ldelem_R8);
                    break;

                case MetadataType.Array:
                case MetadataType.Class:
                case MetadataType.Object:
                case MetadataType.String:
                    ilProcessor.Emit(OpCodes.Ldelem_Ref);
                    break;

                default:
                    ilProcessor.Emit(OpCodes.Ldelem_Any, elementType);
                    break;
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(elementType);
        }

        protected void Ldelema(TypeReference elementType)
        {
            Contract.Requires(elementType != null);
            ilProcessor.Emit(OpCodes.Ldelema, elementType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(elementType);
        }

        protected void Ldfld(FieldReference field)
        {
            Contract.Requires(field != null);

            ilProcessor.Emit(OpCodes.Ldfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldflda(FieldReference field)
        {
            Contract.Requires(field != null);

            ilProcessor.Emit(OpCodes.Ldflda, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldftn(MethodReference function)
        {
            Contract.Requires(function != null);

            ilProcessor.Emit(OpCodes.Ldftn, function);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldind(TypeReference type)
        {
            Contract.Requires(type != null);

            switch (type.MetadataType)
            {
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                case MetadataType.Pointer:
                    ilProcessor.Emit(OpCodes.Ldind_I);
                    break;

                case MetadataType.Boolean:
                case MetadataType.Byte:
                    ilProcessor.Emit(OpCodes.Ldind_U1);
                    break;

                case MetadataType.SByte:
                    ilProcessor.Emit(OpCodes.Ldind_I1);
                    break;

                case MetadataType.Char:
                case MetadataType.UInt16:
                    ilProcessor.Emit(OpCodes.Ldind_U2);
                    break;

                case MetadataType.Int16:
                    ilProcessor.Emit(OpCodes.Ldind_I2);
                    break;

                case MetadataType.UInt32:
                    ilProcessor.Emit(OpCodes.Ldind_U4);
                    break;

                case MetadataType.Int32:
                    ilProcessor.Emit(OpCodes.Ldind_I4);
                    break;

                case MetadataType.UInt64:
                case MetadataType.Int64:
                    ilProcessor.Emit(OpCodes.Ldind_I8);
                    break;

                case MetadataType.Single:
                    ilProcessor.Emit(OpCodes.Ldind_R4);
                    break;

                case MetadataType.Double:
                    ilProcessor.Emit(OpCodes.Ldind_R8);
                    break;

                default:
                    ilProcessor.Emit(OpCodes.Ldobj, type);
                    break;
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
            Assembly.AddTypeUsage(type);
        }

        protected void Ldlen()
        {
            ilProcessor.Emit(OpCodes.Ldlen);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldloc(int index)
        {
            if (index < 4)
            {
                switch (index)
                {
                    case 0:
                        ilProcessor.Emit(OpCodes.Ldloc_0);
                        break;

                    case 1:
                        ilProcessor.Emit(OpCodes.Ldloc_1);
                        break;

                    case 2:
                        ilProcessor.Emit(OpCodes.Ldloc_2);
                        break;

                    case 3:
                        ilProcessor.Emit(OpCodes.Ldloc_3);
                        break;
                }
            }
            else if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldloc_S, body.Variables[index]);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldloc, body.Variables[index]);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldloca(int index)
        {
            if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Ldloca_S, body.Variables[index]);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldloca, body.Variables[index]);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldnull()
        {
            ilProcessor.Emit(OpCodes.Ldnull);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldobj(TypeReference targetType)
        {
            Contract.Requires(targetType != null);

            ilProcessor.Emit(OpCodes.Ldobj, targetType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(targetType);
        }

        protected void Ldsfld(FieldReference field)
        {
            Contract.Requires(field != null);

            ilProcessor.Emit(OpCodes.Ldsfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldsflda(FieldReference field)
        {
            Contract.Requires(field != null);

            ilProcessor.Emit(OpCodes.Ldsflda, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldstr(string str)
        {
            Contract.Requires(str != null);

            ilProcessor.Emit(OpCodes.Ldstr, str);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldtoken(FieldReference fieldReference)
        {
            Contract.Requires(fieldReference != null);

            ilProcessor.Emit(OpCodes.Ldtoken, fieldReference);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldtoken(MethodReference methodReference)
        {
            Contract.Requires(methodReference != null);

            ilProcessor.Emit(OpCodes.Ldtoken, methodReference);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldtoken(TypeReference typeReference)
        {
            Contract.Requires(typeReference != null);

            ilProcessor.Emit(OpCodes.Ldtoken, typeReference);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(typeReference);
        }

        protected void Leave(Instruction target)
        {
            Contract.Requires(target != null);

            if (target.Offset < 256)
            {
                ilProcessor.Emit(OpCodes.Leave_S, target);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Leave, target);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

        }

        protected void Mul()
        {
            ilProcessor.Emit(OpCodes.Mul);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Newobj(MethodReference method)
        {
            Contract.Requires(method != null);

            ilProcessor.Emit(OpCodes.Newobj, method);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Neg()
        {
            ilProcessor.Emit(OpCodes.Neg);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Newarr(TypeReference elementType)
        {
            Contract.Requires(elementType != null);

            ilProcessor.Emit(OpCodes.Newarr, elementType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(elementType);
        }

        protected void Nop()
        {
            ilProcessor.Emit(OpCodes.Nop);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Not()
        {
            ilProcessor.Emit(OpCodes.Not);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Or()
        {
            ilProcessor.Emit(OpCodes.Or);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Pop()
        {
            ilProcessor.Emit(OpCodes.Pop);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Rem()
        {
            ilProcessor.Emit(OpCodes.Rem);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Rem_Un()
        {
            ilProcessor.Emit(OpCodes.Rem_Un);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ret()
        {
            ilProcessor.Emit(OpCodes.Ret);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Shl()
        {
            ilProcessor.Emit(OpCodes.Shl);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Shr()
        {
            ilProcessor.Emit(OpCodes.Shr);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Sizeof(TypeReference typeReference)
        {
            Contract.Requires(typeReference != null);

            ilProcessor.Emit(OpCodes.Sizeof, typeReference);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(typeReference);
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

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Stfld(FieldReference field)
        {
            Contract.Requires(field != null);

            ilProcessor.Emit(OpCodes.Stfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Stelem(TypeReference elementType)
        {
            Contract.Requires(elementType != null);

            switch (elementType.MetadataType)
            {
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                case MetadataType.Pointer:
                    ilProcessor.Emit(OpCodes.Stelem_I);
                    break;

                case MetadataType.Boolean:
                case MetadataType.Byte:
                case MetadataType.SByte:
                    ilProcessor.Emit(OpCodes.Stelem_I1);
                    break;

                case MetadataType.Char:
                case MetadataType.UInt16:
                case MetadataType.Int16:
                    ilProcessor.Emit(OpCodes.Stelem_I2);
                    break;

                case MetadataType.UInt32:
                case MetadataType.Int32:
                    ilProcessor.Emit(OpCodes.Stelem_I4);
                    break;

                case MetadataType.UInt64:
                case MetadataType.Int64:
                    ilProcessor.Emit(OpCodes.Stelem_I8);
                    break;

                case MetadataType.Single:
                    ilProcessor.Emit(OpCodes.Stelem_R4);
                    break;

                case MetadataType.Double:
                    ilProcessor.Emit(OpCodes.Stelem_R8);
                    break;

                case MetadataType.Array:
                case MetadataType.Class:
                case MetadataType.Object:
                case MetadataType.String:
                    ilProcessor.Emit(OpCodes.Stelem_Ref);
                    break;

                default:
                    ilProcessor.Emit(OpCodes.Stelem_Any, elementType);
                    break;
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
            Assembly.AddTypeUsage(elementType);
        }

        protected void Stloc(int index)
        {
            if (index < 4)
            {
                switch (index)
                {
                    case 0:
                        ilProcessor.Emit(OpCodes.Stloc_0);
                        break;

                    case 1:
                        ilProcessor.Emit(OpCodes.Stloc_1);
                        break;

                    case 2:
                        ilProcessor.Emit(OpCodes.Stloc_2);
                        break;

                    case 3:
                        ilProcessor.Emit(OpCodes.Stloc_3);
                        break;
                }
            }
            else if (index < 256)
            {
                ilProcessor.Emit(OpCodes.Stloc_S, (byte)index);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Stloc, index);
            }

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Stsfld(FieldReference field)
        {
            Contract.Requires(field != null);

            ilProcessor.Emit(OpCodes.Stsfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Sub()
        {
            ilProcessor.Emit(OpCodes.Sub);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Tail()
        {
            ilProcessor.Emit(OpCodes.Tail);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Unbox(TypeReference targetType)
        {
            Contract.Requires(targetType != null);

            ilProcessor.Emit(OpCodes.Unbox, targetType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(targetType);
        }

        protected void Unbox_Any(TypeReference targetType)
        {
            Contract.Requires(targetType != null);

            ilProcessor.Emit(OpCodes.Unbox_Any, targetType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;

            Assembly.AddTypeUsage(targetType);
        }

        protected void Xor()
        {
            ilProcessor.Emit(OpCodes.Xor);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        #endregion
    }
}
