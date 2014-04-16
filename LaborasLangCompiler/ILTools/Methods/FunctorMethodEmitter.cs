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
    internal class FunctorMethodEmitter : MethodEmitter
    {
        #region Factories

        static public MethodDefinition EmitConstructor(TypeEmitter declaringType, FieldDefinition objectInstanceField, 
            FieldDefinition functionPtrField)
        {
            var definition = new FunctorMethodEmitter(declaringType, ".ctor", declaringType.Assembly.TypeToTypeReference(typeof(void)),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            definition.EmitConstructorBody(objectInstanceField, functionPtrField);

            return definition.Get();
        }

        static public MethodDefinition EmitInvoke(TypeEmitter declaringType, FieldDefinition objectInstanceField, 
            FieldDefinition functionPtrField, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var definition = new FunctorMethodEmitter(declaringType, "Invoke", returnType, MethodAttributes.Public);

            definition.EmitInvokeBody(objectInstanceField, functionPtrField, returnType, arguments);

            return definition.Get();
        }

        static public MethodDefinition EmitAsDelegate(TypeEmitter declaringType, TypeDefinition delegateType,
            FieldDefinition objectInstanceField, FieldDefinition functionPtrField)
        {
            var definition = new FunctorMethodEmitter(declaringType, "AsDelegate", delegateType, MethodAttributes.Public);

            definition.EmitAsDelegate(objectInstanceField, functionPtrField, delegateType);

            return definition.Get();
        }

        #endregion

        private FunctorMethodEmitter(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes) :
            base(declaringType, name, returnType, methodAttributes)
        {
        }

        private void EmitConstructorBody(FieldDefinition objectInstanceField, FieldDefinition functionPtrField)
        {
            var objectInstanceArgument = AddArgument(Assembly.TypeToTypeReference(typeof(object)), "objectInstance");
            var functionPtrArgument = AddArgument(Assembly.TypeToTypeReference(typeof(System.IntPtr)), "functionPtr");

            Ldarg(0);
            Ldarg(objectInstanceArgument.Index + 1);
            Stfld(objectInstanceField);
            
            Ldarg(0);
            Ldarg(functionPtrArgument.Index + 1);
            Stfld(functionPtrField);

            Ret();
        }

        private void EmitInvokeBody(FieldDefinition objectInstanceField, FieldDefinition functionPtrField, TypeReference returnType,
            IReadOnlyList<TypeReference> arguments)
        {
            var staticCallsite = new CallSite(returnType);
            var instanceCallsite = new CallSite(returnType);

            foreach (var parameterType in arguments)
            {
                AddArgument(parameterType);
                staticCallsite.Parameters.Add(new ParameterDefinition(parameterType));
                instanceCallsite.Parameters.Add(new ParameterDefinition(parameterType));
            }

            staticCallsite.HasThis = false;
            instanceCallsite.HasThis = true;

            var labelAfterConditionalBlock = CreateLabel();

            // if (objectInstance != 0)
            Ldarg(0);
            Ldfld(objectInstanceField);
            Dup();
            Brtrue(labelAfterConditionalBlock);

            {
                Pop();  // This pops earlier-duplicated objectInstance field

                for (int i = 0; i < arguments.Count; i++)
                {
                    Ldarg(i + 1);
                }

                Ldarg(0);
                Ldfld(functionPtrField);

                Calli(staticCallsite);
                Ret();
            }
            // else
            {
                Emit(labelAfterConditionalBlock);

                for (int i = 0; i < arguments.Count; i++)
                {
                    Ldarg(i + 1);
                }

                Ldarg(0);
                Ldfld(functionPtrField);

                Calli(instanceCallsite);
                Ret();
            }
        }

        private void EmitAsDelegate(FieldReference objectInstanceField, FieldReference functionPtrField, TypeReference delegateType)
        {
            var ctor = AssemblyRegistry.GetMethods(Assembly, delegateType, ".ctor").Single(x => x.Parameters.Count == 2 &&
                x.Parameters[0].ParameterType.FullName == "System.Object" && x.Parameters[1].ParameterType.FullName == "System.IntPtr");

            Ldarg(0);
            Ldfld(objectInstanceField);

            Ldarg(0);
            Ldfld(functionPtrField);

            Newobj(ctor);
            Ret();
        }
    }
}
