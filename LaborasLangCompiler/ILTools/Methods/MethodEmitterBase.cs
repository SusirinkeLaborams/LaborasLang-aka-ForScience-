using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.Methods
{
    internal abstract class MethodEmitterBase
    {
        protected MethodDefinition methodDefinition;
        protected MethodBody body;
        protected ILProcessor ilProcessor;

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
        }

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

        protected void Emit(Instruction instruction)
        {
            body.Instructions.Add(instruction);
        }

        protected Instruction CreateLabel()
        {
            return Instruction.Create(OpCodes.Nop);
        }

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

        protected void Box(TypeReference type)
        {
            ilProcessor.Emit(OpCodes.Box, type);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
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

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
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

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
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

            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Call(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Call, method);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Calli(CallSite callSite)
        {
            ilProcessor.Emit(OpCodes.Calli, callSite);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Callvirt(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Callvirt, method);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Castclass(TypeReference targetType)
        {
            ilProcessor.Emit(OpCodes.Castclass, targetType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
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

        protected void Clt()
        {
            ilProcessor.Emit(OpCodes.Clt);
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

        protected void Ldfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldflda(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldflda, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldftn(MethodReference function)
        {
            ilProcessor.Emit(OpCodes.Ldftn, function);
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
            ilProcessor.Emit(OpCodes.Ldobj);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldsfld(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldsfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldsflda(FieldReference field)
        {
            ilProcessor.Emit(OpCodes.Ldsflda, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Ldstr(string str)
        {
            ilProcessor.Emit(OpCodes.Ldstr, str);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Mul()
        {
            ilProcessor.Emit(OpCodes.Mul);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Newobj(MethodReference method)
        {
            ilProcessor.Emit(OpCodes.Newobj, method);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Neg()
        {
            ilProcessor.Emit(OpCodes.Neg);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Newarr(TypeReference arrayType)
        {
            ilProcessor.Emit(OpCodes.Newarr, arrayType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
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
            ilProcessor.Emit(OpCodes.Stfld, field);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Stelem_Any(TypeReference valueType)
        {
            ilProcessor.Emit(OpCodes.Stelem_Any, valueType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Stelem_Ref()
        {
            ilProcessor.Emit(OpCodes.Stelem_Ref);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
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
            ilProcessor.Emit(OpCodes.Unbox, targetType);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        protected void Xor()
        {
            ilProcessor.Emit(OpCodes.Xor);
            body.Instructions[body.Instructions.Count - 1].SequencePoint = CurrentSequencePoint;
        }

        #endregion
    }
}
