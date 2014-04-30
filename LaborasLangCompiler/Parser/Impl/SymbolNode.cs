using Mono.Cecil;
using NPEG;
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
    interface ISymbolNode : IExpressionNode
    {
        SymbolNodeType SymbolType { get; }
    }
    class NamespaceNode : ISymbolNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public TypeReference ReturnType { get { return null; } }
        public SymbolNodeType SymbolType { get { return SymbolNodeType.Namespace; } }
        public string FullNamespace { get; private set; }
        public NamespaceNode(string namespaze)
        {
            FullNamespace = namespaze;
        }
    }
    class TypeNode : ISymbolNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public TypeReference ReturnType { get { return null; } }
        public SymbolNodeType SymbolType { get { return SymbolNodeType.Type; } }
        public TypeReference ParsedType { get; private set; }
        public TypeNode(TypeReference type)
        {
            ParsedType = type;
        }
    }
}
