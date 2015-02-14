using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeReference ExpressionReturnType { get { return null; } }
        public string Name { get; private set; }
        public ContextNode Scope { get; private set; }
        public override bool IsGettable { get { return false; } }
        public override bool IsSettable { get { return false; } }

        protected SymbolNode(string value, ContextNode scope, SequencePoint point)
            : base(point)
        {
            Name = value;
            Scope = scope;
        }
        public static SymbolNode Parse(Parser parser, ContextNode parent, AstNode lexerNode)
        {
            return new SymbolNode(lexerNode.Content.ToString(), parent, parser.GetSequencePoint(lexerNode));
        }
        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }

    class NamespaceNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeReference ExpressionReturnType { get { return null; } }
        public Namespace Namespace { get; private set; }
        public override bool IsGettable { get { return false; } }
        public override bool IsSettable { get { return false; } }
        public NamespaceNode(Namespace namespaze, SequencePoint point)
            : base(point)
        {
            this.Namespace = namespaze;
        }
        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
