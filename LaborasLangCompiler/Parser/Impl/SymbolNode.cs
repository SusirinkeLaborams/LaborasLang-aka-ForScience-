using LaborasLangCompiler.ILTools;
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
        public override TypeWrapper TypeWrapper { get { return null; } }
        public string Value { get; private set; }
        protected SymbolNode(string value, SequencePoint point) : base(point)
        {
            Value = value;
            
        }
        public static new SymbolNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            return new SymbolNode(lexerNode.Content.ToString(), parser.GetSequencePoint(lexerNode));
        }
    }
    class NamespaceNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeWrapper TypeWrapper { get { return null; } }
        public NamespaceWrapper Namespace { get; private set; }
        public NamespaceNode(NamespaceWrapper namespaze, SequencePoint point) : base(point)
        {
            this.Namespace = namespaze;
        }
    }
    class SymbolCallNode : SymbolNode
    {
        public List<ExpressionNode> Arguments { get; private set; }
        protected SymbolCallNode(string name, List<ExpressionNode> args, SequencePoint point) : base(name, point)
        {
            Arguments = args;
        }
        public static new SymbolCallNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            string name = lexerNode.Children[0].Content.ToString();
            var args = new List<ExpressionNode>();
            foreach(var node in lexerNode.Children[1].Children)
            {
                args.Add(ExpressionNode.Parse(parser, parent, node));
            }
            return new SymbolCallNode(name, args, parser.GetSequencePoint(lexerNode));
        }
    }
}
