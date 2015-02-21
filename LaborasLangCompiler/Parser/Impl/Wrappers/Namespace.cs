using LaborasLangCompiler.Codegen;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class Namespace
    {
        public readonly string Name;
        private readonly AssemblyEmitter assembly;

        public Namespace GetContainedNamespace(string name)
        {
            var full = Name + "." + name;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new Namespace(full, assembly);
            else
                return null;
        }

        public TypeReference GetContainedType(string name)
        {
            var full = Name + "." + name;
            return AssemblyRegistry.FindType(assembly, full);
        }

        public Namespace(string namespaze, AssemblyEmitter assembly)
        {
            this.assembly = assembly;
            Name = namespaze;
        }
    }
}
