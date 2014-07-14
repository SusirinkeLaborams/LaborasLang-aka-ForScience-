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

        protected MethodEmitterBase(TypeEmitter declaringType, string name, TypeReference returnType,
                                MethodAttributes methodAttributes = MethodAttributes.Private)
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

        #region IL Instructions

        protected void Add()
        {
            ilProcessor.Emit(OpCodes.Add);
        }

        protected void And()
        {
            ilProcessor.Emit(OpCodes.And);
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
                ilProcessor.Emit(OpCodes.Ldarg_S, methodDefinition.Parameters[index]);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldarg, methodDefinition.Parameters[index]);
            }
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
                ilProcessor.Emit(OpCodes.Ldloc_S, body.Variables[index]);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Ldloc, body.Variables[index]);
            }
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

        protected void Neg()
        {
            ilProcessor.Emit(OpCodes.Neg);
        }

        protected void Newarr(TypeReference arrayType)
        {
            ilProcessor.Emit(OpCodes.Newarr, arrayType);
        }

        protected void Nop()
        {
            ilProcessor.Emit(OpCodes.Nop);
        }

        protected void Not()
        {
            ilProcessor.Emit(OpCodes.Not);
        }

        protected void Or()
        {
            ilProcessor.Emit(OpCodes.Or);
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

        protected void Shl()
        {
            ilProcessor.Emit(OpCodes.Shl);
        }

        protected void Shr()
        {
            ilProcessor.Emit(OpCodes.Shr);
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

        protected void Stelem_Any(TypeReference valueType)
        {
            ilProcessor.Emit(OpCodes.Stelem_Any, valueType);
        }

        protected void Stelem_Ref()
        {
            ilProcessor.Emit(OpCodes.Stelem_Ref);
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
                ilProcessor.Emit(OpCodes.Stloc_S, (byte)index);
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

        protected void Tail()
        {
            ilProcessor.Emit(OpCodes.Tail);
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
