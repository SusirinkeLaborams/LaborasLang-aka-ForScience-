using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalType : TypeWrapper
    {
        public TypeReference TypeReference { get; private set; }

        private AssemblyEmitter assembly;

        public ExternalType(AssemblyEmitter assembly, TypeReference type)
        {
            TypeReference = type;
            this.assembly = assembly;
        }

        public TypeWrapper GetContainedType(string name)
        {
            var type = AssemblyRegistry.FindType(assembly, TypeReference + "." + name);
            if (type != null)
                return new ExternalType(assembly, type);
            else
                return null;
        }
        public FieldWrapper GetField(string name)
        {
            return new ExternalField(AssemblyRegistry.GetField(assembly, TypeReference, name));
        }
        public MethodWrapper GetMethod(string name)
        {
            return new ExternalMethod(AssemblyRegistry.GetMethod(assembly, TypeReference, name), assembly);
        }
        public IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return AssemblyRegistry.GetMethods(assembly, TypeReference, name).Select(m => new ExternalMethod(m, assembly));
        }
        public bool IsAssignableTo(TypeWrapper type)
        {
            if (!(type is ExternalType))
                return false;
            var t = ((ExternalType)type);
            return this.IsAssignableTo(type);
        }
    }
}
