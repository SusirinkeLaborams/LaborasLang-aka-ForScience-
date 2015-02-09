using LaborasLangCompiler.Codegen.Methods;
using Mono.Cecil;
using System.Collections.Generic;

namespace LaborasLangCompiler.Codegen.Types
{
    internal class FunctorBaseTypeEmitter : TypeEmitter
    {
        private const TypeAttributes FunctorTypeAttributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit | TypeAttributes.Abstract;
        private const MethodAttributes CtorAttributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        private const MethodAttributes AbstractMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.Abstract;

        public static TypeDefinition Create(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return new FunctorBaseTypeEmitter(assembly, returnType, arguments).typeDefinition;
        }

        private FunctorBaseTypeEmitter(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments) :
            base(assembly, ComputeName(returnType, arguments), "$Functors", FunctorTypeAttributes, assembly.TypeToTypeReference(typeof(object)), false)
        {
            var delegateType = DelegateEmitter.Create(assembly, typeDefinition, returnType, arguments);
            typeDefinition.NestedTypes.Add(delegateType);

            new ConstructorEmitter(this, false, CtorAttributes); // Added to type by emitter
            var invokeMethod = new MethodDefinition("Invoke", AbstractMethodAttributes, returnType);
            
            foreach (var argument in arguments)
            {
                invokeMethod.Parameters.Add(new ParameterDefinition(argument));
            }

            var asDelegateMethod = new MethodDefinition("AsDelegate", AbstractMethodAttributes, delegateType);

            typeDefinition.Methods.Add(invokeMethod);
            typeDefinition.Methods.Add(asDelegateMethod);
        }

        private static string ComputeName(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return ComputeNameFromReturnAndArgumentTypes(returnType, arguments);
        }
    }
}
