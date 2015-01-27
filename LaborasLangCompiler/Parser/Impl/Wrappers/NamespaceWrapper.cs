using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    interface NamespaceWrapper
    {
        string Namespace { get; }
        NamespaceWrapper GetContainedNamespace(string name);
        TypeReference GetContainedType(string name);
    }
}