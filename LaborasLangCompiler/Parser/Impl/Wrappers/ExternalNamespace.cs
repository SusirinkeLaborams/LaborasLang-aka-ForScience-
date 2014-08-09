using LaborasLangCompiler.ILTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalNamespace : NamespaceWrapper
    {
        public string Namespace {get; private set;}

        public NamespaceWrapper GetContainedNamespace(string name)
        {
            var full = Namespace + "." + name;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new ExternalNamespace(full, assembly);
            else
                return null;
        }

        public TypeWrapper GetContainedType(string name)
        {
            var full = Namespace + "." + name;
            var type = AssemblyRegistry.FindType(assembly, full);
            if (type != null)
                return new ExternalType(assembly, type);
            else
                return null;
        }

        private AssemblyEmitter assembly;

        private ExternalNamespace(string namespaze, AssemblyEmitter assembly)
        {
            Namespace = namespaze;
            this.assembly = assembly;
        }
    }
}
