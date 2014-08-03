using LaborasLangCompiler.ILTools.Types;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.Methods
{
    internal class FunctorMethodEmitter : MethodEmitterBase
    {
        #region Factories

        static public void EmitConstructor(TypeEmitter declaringType, MethodReference targetMethod)
        {
            var definition = new FunctorMethodEmitter(declaringType, ".ctor", declaringType.Assembly.TypeToTypeReference(typeof(void)),
                MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            
            definition.EmitConstructorBody(targetMethod);
        }

        static public void EmitInvoke(TypeEmitter declaringType, MethodReference targetMethod)
        {
            var definition = new FunctorMethodEmitter(declaringType, "Invoke", targetMethod.ReturnType, 
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);

            definition.EmitInvokeBody(targetMethod);
        }

        static public void EmitAsDelegate(TypeEmitter declaringType, TypeReference delegateType, MethodReference targetMethod)
        {
            var definition = new FunctorMethodEmitter(declaringType, "AsDelegate", delegateType,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);

            definition.EmitAsDelegate(delegateType, targetMethod);
        }

        #endregion

        private FieldReference ThisField
        {
            get
            {
                return AssemblyRegistry.GetField(Assembly, DeclaringType, "$this");
            }
        }

        private FunctorMethodEmitter(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes) :
            base(declaringType, name, returnType, methodAttributes)
        {
        }

        private void EmitConstructorBody(MethodReference targetMethod)
        {
            var baseCtor = AssemblyRegistry.GetMethod(Assembly, DeclaringType.BaseType, ".ctor");

            Ldarg(0);
            Call(baseCtor);

            if (targetMethod.HasThis)
            {
                AddArgument(targetMethod.DeclaringType, "$this");

                var thisField = new FieldDefinition("$this", FieldAttributes.Private, targetMethod.DeclaringType);
                DeclaringType.AddField(thisField);

                Ldarg(0);
                Ldarg(1);
                Stfld(thisField);
            }

            Ret();
        }

        private void EmitInvokeBody(MethodReference targetMethod)
        {
            if (targetMethod.HasThis)
            {
                Ldarg(0);
                Ldfld(ThisField);
            }

            for (int i = 0; i < targetMethod.Parameters.Count; i++)
            {
                AddArgument(targetMethod.Parameters[i]);
                Ldarg(i + 1);
            }

            Tail();

            if (targetMethod.Resolve().IsVirtual)
            {
                Callvirt(targetMethod);
            }
            else
            {
                Call(targetMethod);
            }

            Ret();
        }

        private void EmitAsDelegate(TypeReference delegateType, MethodReference targetMethod)
        {
            var ctor = AssemblyRegistry.GetMethod(Assembly, delegateType, ".ctor");

            if (targetMethod.HasThis)
            {
                Ldarg(0);
                Ldfld(ThisField);
            }
            else
            {
                Ldnull();
            }

            Ldftn(targetMethod);
            Newobj(ctor);

            Ret();
        }
    }
}
