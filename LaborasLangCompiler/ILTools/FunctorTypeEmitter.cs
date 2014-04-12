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
        public static string ComputeClassName(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = new StringBuilder("$Functor$" + returnType.FullName + "$");

            for (int i = 0; i < arguments.Count; i++)
            {
                if (i != 0)
                {
                    name.Append("_");
                }

                name.Append(arguments[i].FullName);
            }

            return name.ToString();
        }

        public static TypeDefinition Create(AssemblyRegistry assemblyRegistry, AssemblyEmitter assembly,
            TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            return new FunctorTypeEmitter(assemblyRegistry, assembly, returnType, arguments).typeDefinition;
        }

        private FunctorTypeEmitter(AssemblyRegistry assemblyRegistry, AssemblyEmitter assembly,
            TypeReference returnType, IReadOnlyList<TypeReference> arguments) :
            base(assembly, ComputeClassName(returnType, arguments), "$Functors")
        {
            var delegateType = DelegateEmitter.Create(assemblyRegistry, assembly, "$Functors", returnType, arguments);
            typeDefinition.NestedTypes.Add(delegateType);

            var objectInstanceField = new FieldDefinition("objectInstance", FieldAttributes.Private | FieldAttributes.InitOnly,
                assembly.Import(assemblyRegistry.ImportType(typeof(object))));

            var functionPtrField = new FieldDefinition("functionPtr", FieldAttributes.Private | FieldAttributes.InitOnly,
                assembly.Import(assemblyRegistry.ImportType(typeof(IntPtr))));

            typeDefinition.Fields.Add(objectInstanceField);
            typeDefinition.Fields.Add(functionPtrField);

            FunctorMethodEmitter.EmitConstructor(assemblyRegistry, this, objectInstanceField, functionPtrField);
            FunctorMethodEmitter.EmitInvoke(assemblyRegistry, this, objectInstanceField, functionPtrField, returnType, arguments);
            FunctorMethodEmitter.EmitAsDelegate(assemblyRegistry, this, delegateType, objectInstanceField, functionPtrField);
        }
    }
}
