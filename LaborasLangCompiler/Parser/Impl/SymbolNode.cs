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
        public override TypeWrapper ReturnType { get { return null; } }
        public string Value { get; private set; }
        protected SymbolNode(string value, SequencePoint point) : base(point)
        {
            Value = value;
            
        }
        public static new SymbolNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            return new SymbolNode(parser.ValueOf(lexerNode), parser.GetSequencePoint(lexerNode));
        }
    }
    class NamespaceNode : SymbolNode
    {
        public NamespaceNode(string name, SequencePoint point) : base(name, point) { }
    }
    class SymbolCallNode : SymbolNode
    {
        public List<IExpressionNode> Arguments { get; private set; }
        protected SymbolCallNode(string name, List<IExpressionNode> args, SequencePoint point) : base(name, point)
        {
            Arguments = args;
        }
        public static new SymbolCallNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            string name = parser.ValueOf(lexerNode.Children[0]);
            var args = new List<IExpressionNode>();
            foreach(var node in lexerNode.Children[1].Children)
            {
                args.Add(ExpressionNode.Parse(parser, parent, node));
            }
            return new SymbolCallNode(name, args, parser.GetSequencePoint(lexerNode));
        }
    }
    class TypeNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public TypeWrapper ReturnType { get { return null; } }
        public TypeWrapper ParsedType { get; private set; }
        public TypeNode(TypeWrapper type, SequencePoint point)
            : base(point)
        {
            ParsedType = type;
        }
        public static TypeWrapper Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
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
                var args = new List<TypeReference>();
                foreach(var arg in lexerNode.Children[1].Children)
                {
                    args.Add(TypeNode.Parse(parser, parent, arg));
                }
                return AssemblyRegistry.GetFunctorType(parser.Assembly, ret, args);
            }
        }
    }
}
