using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class Namespace : ExternalWrapperBase
    {
        public string Name {get; private set;}

        public Namespace GetContainedNamespace(string name)
        {
            var full = Name + "." + name;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new Namespace(full, Assembly);
            else
                return null;
        }

        public TypeReference GetContainedType(string name)
        {
            var full = Name + "." + name;
            return AssemblyRegistry.FindType(Assembly, full);
        }

        public Namespace(string namespaze, AssemblyEmitter assembly)
            : base(assembly)
        {
            Name = namespaze;
        }
    }
}
