using LaborasLangCompiler.ILTools.Methods;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.Types
{
    internal class FunctorTypeEmitter : TypeEmitter
    {
        private const TypeAttributes FunctorTypeAttributes = TypeAttributes.SequentialLayout | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        public static TypeDefinition Create(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return new FunctorTypeEmitter(assembly, returnType, arguments).typeDefinition;
        }

        private FunctorTypeEmitter(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments) :
            base(assembly, ComputeName(returnType, arguments), "$Functors", FunctorTypeAttributes, AssemblyRegistry.GetType(assembly, "System.ValueType"))
        {
            var delegateType = DelegateEmitter.Create(assembly, typeDefinition, returnType, arguments);
            typeDefinition.NestedTypes.Add(delegateType);

            var objectInstanceField = new FieldDefinition("objectInstance", FieldAttributes.Private | FieldAttributes.InitOnly,
                Assembly.TypeToTypeReference(typeof(object)));

            var functionPtrField = new FieldDefinition("functionPtr", FieldAttributes.Private | FieldAttributes.InitOnly,
                Assembly.TypeToTypeReference(typeof(IntPtr)));

            typeDefinition.Fields.Add(objectInstanceField);
            typeDefinition.Fields.Add(functionPtrField);

            FunctorMethodEmitter.EmitConstructor(this, objectInstanceField, functionPtrField);
            FunctorMethodEmitter.EmitInvoke(this, objectInstanceField, functionPtrField, returnType, arguments);
            FunctorMethodEmitter.EmitAsDelegate(this, delegateType, objectInstanceField, functionPtrField);
        }

        private static string ComputeName(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return ComputeNameFromReturnAndArgumentTypes(returnType, arguments);
        }
    }
}
