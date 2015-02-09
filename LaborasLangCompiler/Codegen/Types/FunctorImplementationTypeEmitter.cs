using LaborasLangCompiler.Codegen.Methods;
using Mono.Cecil;
using System.Linq;

namespace LaborasLangCompiler.Codegen.Types
{
    internal class FunctorImplementationTypeEmitter : TypeEmitter
    {
        private const TypeAttributes FunctorImplementationTypeAttributes = TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        public static TypeDefinition Create(AssemblyEmitter assembly, TypeReference declaringType, MethodReference targetMethod)
        {
            var baseType = AssemblyRegistry.GetFunctorType(assembly, targetMethod);
            assembly.AddTypeIfNotAdded(baseType.Resolve());
            return new FunctorImplementationTypeEmitter(assembly, declaringType, baseType, targetMethod).typeDefinition;
        }

        private FunctorImplementationTypeEmitter(AssemblyEmitter assembly, TypeReference declaringType, TypeReference baseType, MethodReference targetMethod) :
            base(assembly, ComputeName(targetMethod), "", FunctorImplementationTypeAttributes, baseType, false)
        {
            ((TypeDefinition)declaringType).NestedTypes.Add(typeDefinition);
            var delegateType = baseType.Resolve().NestedTypes.Single();

            FunctorMethodEmitter.EmitConstructor(this, targetMethod);
            FunctorMethodEmitter.EmitInvoke(this, targetMethod);
            FunctorMethodEmitter.EmitAsDelegate(this, delegateType, targetMethod);
        }

        private static string ComputeName(MethodReference targetMethod)
        {
            return targetMethod.Name + ComputeNameArgumentTypes(targetMethod.Parameters.Select(x => x.ParameterType).ToList()) + "$Invoker";
        }
    }
}
