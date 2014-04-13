using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class FunctorTypeEmitter : TypeEmitter
    {
        private const TypeAttributes FunctorTypeAttributes = TypeAttributes.SequentialLayout | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        public static TypeDefinition Create(AssemblyRegistry assemblyRegistry, AssemblyEmitter assembly,
            TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return new FunctorTypeEmitter(assemblyRegistry, assembly, returnType, arguments).typeDefinition;
        }

        private FunctorTypeEmitter(AssemblyRegistry assemblyRegistry, AssemblyEmitter assembly,
            TypeReference returnType, IReadOnlyList<TypeReference> arguments) :
            base(assembly, ComputeName(returnType, arguments), "$Functors", FunctorTypeAttributes, assemblyRegistry.GetType("System.ValueType"))
        {
            var delegateType = DelegateEmitter.Create(assemblyRegistry, assembly, typeDefinition, returnType, arguments);
            typeDefinition.NestedTypes.Add(delegateType);

            var objectInstanceField = new FieldDefinition("objectInstance", FieldAttributes.Private | FieldAttributes.InitOnly,
                Module.Import(typeof(object)));

            var functionPtrField = new FieldDefinition("functionPtr", FieldAttributes.Private | FieldAttributes.InitOnly,
                Module.Import(typeof(IntPtr)));

            typeDefinition.Fields.Add(objectInstanceField);
            typeDefinition.Fields.Add(functionPtrField);

            FunctorMethodEmitter.EmitConstructor(assemblyRegistry, this, objectInstanceField, functionPtrField);
            FunctorMethodEmitter.EmitInvoke(assemblyRegistry, this, objectInstanceField, functionPtrField, returnType, arguments);
            FunctorMethodEmitter.EmitAsDelegate(assemblyRegistry, this, delegateType, objectInstanceField, functionPtrField);
        }

        private static string ComputeName(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return ComputeNameFromReturnAndArgumentTypes(returnType, arguments);
        }
    }
}
