using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
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
        public static SymbolNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            return new SymbolNode(parser.ValueOf(lexerNode), parser.GetSequencePoint(lexerNode));
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
            string name = parser.ValueOf(lexerNode.Children[0]);
            var args = new List<ExpressionNode>();
            foreach(var node in lexerNode.Children[1].Children)
            {
                args.Add(ExpressionNode.Parse(parser, parent, node));
            }
            return new SymbolCallNode(name, args, parser.GetSequencePoint(lexerNode));
        }
    }
    class TypeNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeWrapper TypeWrapper { get { return null; } }
        public TypeWrapper ParsedType { get; private set; }
        public TypeNode(TypeWrapper type, SequencePoint point)
            : base(point)
        {
            ParsedType = type;
        }
        public static TypeWrapper Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if (lexerNode.Token.Name == Lexer.FunctionType)
                return TypeNode.Parse(parser, parent, lexerNode.Children[0]);

            var ret = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]).ExtractType();

            if (lexerNode.Children.Count == 1)
            {
                return ret;
            }
            else
            {
                var args = new List<TypeWrapper>();
                foreach(var arg in lexerNode.Children[1].Children)
                {
                    args.Add(TypeNode.Parse(parser, parent, arg));
                }
                return new FunctorTypeWrapper(parser.Assembly, ret, args);
            }
        }
    }
}
