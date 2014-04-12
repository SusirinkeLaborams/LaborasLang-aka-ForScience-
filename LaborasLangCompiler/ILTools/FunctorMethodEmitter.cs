using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class FunctorMethodEmitter : MethodEmitter
    {
        #region Factories

        static public MethodDefinition EmitConstructor(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType, 
            FieldDefinition objectInstanceField, FieldDefinition functionPtrField)
        {
            var definition = new FunctorMethodEmitter(assemblyRegistry, declaringType, ".ctor", assemblyRegistry.ImportType(typeof(void)),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            definition.EmitConstructorBody(objectInstanceField, functionPtrField);

            return definition.Get();
        }

        static public MethodDefinition EmitInvoke(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType,
            FieldDefinition objectInstanceField, FieldDefinition functionPtrField, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var definition = new FunctorMethodEmitter(assemblyRegistry, declaringType, "Invoke", returnType, MethodAttributes.Public);

            definition.EmitInvokeBody(objectInstanceField, functionPtrField, returnType, arguments);

            return definition.Get();
        }

        static public MethodDefinition EmitAsDelegate(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType, TypeDefinition delegateType,
            FieldDefinition objectInstanceField, FieldDefinition functionPtrField)
        {
            var definition = new FunctorMethodEmitter(assemblyRegistry, declaringType, "AsDelegate", delegateType, MethodAttributes.Public);

            definition.EmitAsDelegate(objectInstanceField, functionPtrField, delegateType);

            return definition.Get();
        }

        #endregion

        private FunctorMethodEmitter(AssemblyRegistry assemblyRegistry, TypeEmitter declaringType, string name,
            TypeReference returnType, MethodAttributes methodAttributes) :
            base(assemblyRegistry, declaringType, name, returnType, methodAttributes)
        {
        }

        private void EmitConstructorBody(FieldDefinition objectInstanceField, FieldDefinition functionPtrField)
        {
            var objectInstanceArgument = AddArgument(assemblyRegistry.ImportType(typeof(object)), "objectInstance");
            var functionPtrArgument = AddArgument(assemblyRegistry.ImportType(typeof(System.IntPtr)), "functionPtr");

            Ldarg(0);
            Ldarg(objectInstanceArgument.Index);
            Stfld(objectInstanceField);
            
            Ldarg(0);
            Ldarg(functionPtrArgument.Index);
            Stfld(functionPtrField);

            Ret();
        }

        private void EmitInvokeBody(FieldDefinition objectInstanceField, FieldDefinition functionPtrField, TypeReference returnType,
            IReadOnlyList<TypeReference> arguments)
        {
            var callsite = new CallSite(returnType);

            foreach (var parameterType in arguments)
            {
                AddArgument(parameterType);
                callsite.Parameters.Add(new ParameterDefinition(parameterType));
            }

            var labelAfterConditionalBlock = CreateLabel();

            // Check if object is not null and if it's not, leave it in the stack
            Ldarg(0);
            Ldfld(objectInstanceField);
            Dup();
            Brtrue(labelAfterConditionalBlock);
            Pop();

            Emit(labelAfterConditionalBlock);

            for (int i = 0; i < arguments.Count; i++)
            {
                Ldarg(i + 1);
            }

            Calli(callsite);
            Ret();
        }

        private void EmitAsDelegate(FieldDefinition objectInstanceField, FieldDefinition functionPtrField, TypeDefinition delegateType)
        {
            var ctor = delegateType.Methods.Single(x => x.Parameters.Count == 2 &&
                x.Parameters[0].ParameterType.FullName == "System.Object" && x.Parameters[1].ParameterType.FullName == "System.IntPtr");

            Ldarg(0);
            Dup();
            Ldfld(objectInstanceField);
            Ldfld(functionPtrField);

            Newobj(Import(ctor));
        }
    }
}
