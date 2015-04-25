using LaborasLangCompiler.Codegen.Types;
using Mono.Cecil;

namespace LaborasLangCompiler.Codegen.Methods
{
    internal class FunctorMethodEmitter : MethodEmitterBase
    {
        #region Factories

        static public void EmitConstructor(TypeEmitter declaringType, MethodReference targetMethod)
        {
            var definition = new FunctorMethodEmitter(declaringType, ".ctor", declaringType.Assembly.TypeSystem.Void,
                MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            
            definition.EmitConstructorBody(targetMethod);
        }

        static public void EmitInvoke(TypeEmitter declaringType, MethodReference targetMethod)
        {
            var definition = new FunctorMethodEmitter(declaringType, "Invoke", targetMethod.GetReturnType(), 
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
            var baseCtor = AssemblyRegistry.GetConstructor(Assembly, DeclaringType.BaseType);

            Ldarg(0);
            Call(methodDefinition.DeclaringType, baseCtor);

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

                if (ThisField.FieldType.IsValueType)
                {
                    Ldflda(ThisField);
                }
                else
                {
                    Ldfld(ThisField);
                }
            }

            for (int i = 0; i < targetMethod.Parameters.Count; i++)
            {
                AddArgument(targetMethod.Parameters[i]);
                Ldarg(i + 1);
            }

            Tail();
            Call(targetMethod.HasThis ? ThisField.FieldType : null, targetMethod);

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
