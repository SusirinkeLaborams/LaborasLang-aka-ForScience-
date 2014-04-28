using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    enum SymbolNodeType
    {
        Namespace,
        Type,
        LValue
    }
    interface ISymbolNode
    {
        SymbolNodeType SymbolType { get; }
    }
    class NamespaceNode : ISymbolNode
    {
        public SymbolNodeType SymbolType { get { return SymbolNodeType.Namespace; } }
        public string FullNamespace { get; private set; }
        public NamespaceNode(string namespaze)
        {
            FullNamespace = namespaze;
        }
    }
    class TypeNode : ISymbolNode
    {
        public SymbolNodeType SymbolType { get { return SymbolNodeType.Type; } }
        public TypeReference ReturnType { get; private set; }
        public TypeNode(TypeReference type)
        {
            ReturnType = type;
        }
    }
    abstract class SymbolNode
    {

    }
}
