using LaborasLangCompiler.ILTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalNamespace : ExternalWrapperBase, NamespaceWrapper
    {
        public string Namespace {get; private set;}

        public NamespaceWrapper GetContainedNamespace(string name)
        {
            var full = Namespace + "." + name;
            if (AssemblyRegistry.IsNamespaceKnown(full))
                return new ExternalNamespace(full, Assembly);
            else
                return null;
        }

        public TypeWrapper GetContainedType(string name)
        {
            var full = Namespace + "." + name;
            var type = AssemblyRegistry.FindType(Assembly, full);
            if (type != null)
                return new ExternalType(Assembly, type);
            else
                return null;
        }

        public ExternalNamespace(string namespaze, AssemblyEmitter assembly) : base(assembly)
        {
            Namespace = namespaze;
        }
    }
}
