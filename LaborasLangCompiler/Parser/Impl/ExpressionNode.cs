using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.LexingTools;
using Mono.Cecil.Cil;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ExpressionNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ReturnType { get; }
        protected ExpressionNode(SequencePoint sequencePoint) : base(sequencePoint) { }
        public static IExpressionNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            switch (lexerNode.Token.Name)
            {
                case Lexer.FullSymbol:
                    var tmp = DotOperatorNode.Parse(parser, parent, lexerNode);
                    return tmp.ExtractExpression();
                case Lexer.Symbol:
                    return SymbolNode.Parse(parser, parent, lexerNode);
                case Lexer.Literal:
                    return LiteralNode.Parse(parser, parent, lexerNode);
                case Lexer.Value:
                case Lexer.FunctionArgument:
                    return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                case Lexer.Sum:
                case Lexer.Product:
                case Lexer.BinaryOperationNode:
                case Lexer.BooleanNode:
                case Lexer.Comparison:
                    return BinaryOperatorNode.Parse(parser, parent, lexerNode);
                case Lexer.Function:
                    return FunctionDeclarationNode.Parse(parser, parent, lexerNode);
                case Lexer.PrefixNode:
                case Lexer.SuffixNode:
                    return UnaryOperatorNode.Parse(parser, parent, lexerNode);
                case Lexer.FunctionCall:
                    return MethodCallNode.Parse(parser, parent, lexerNode);
                case Lexer.Assignment:
                    return AssignmentOperatorNode.Parse(parser, parent, lexerNode);
                default:
                    throw new NotImplementedException();
            }
        }
        public override string ToString()
        {
            return String.Format("(ExpressionNode: {0} {1})", ExpressionType, ReturnType);
        }
    }
}
