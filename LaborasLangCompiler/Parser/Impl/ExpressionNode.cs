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
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ExpressionNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public abstract ExpressionNodeType ExpressionType { get; }
        public TypeReference ExpressionReturnType { get { return TypeWrapper != null ? TypeWrapper.TypeReference : null; } }
        public abstract TypeWrapper TypeWrapper { get; }
        protected ExpressionNode(SequencePoint sequencePoint) : base(sequencePoint) { }
        public static ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode, bool allowAmbiguous = false)
        {
            ExpressionNode expression = null;
            switch (lexerNode.Token.Name)
            {
                case Lexer.FullSymbol:
                    expression = DotOperatorNode.Parse(parser, parent, lexerNode).ExtractExpression(allowAmbiguous);
                    break;
                case Lexer.Symbol:
                    expression = SymbolNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.Literal:
                    expression = LiteralNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.Value:
                case Lexer.FunctionArgument:
                    expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0], allowAmbiguous);
                    break;
                case Lexer.Sum:
                case Lexer.Product:
                case Lexer.BinaryOperationNode:
                case Lexer.BooleanNode:
                case Lexer.Comparison:
                    expression = BinaryOperatorNode.Parse(parser, parent, lexerNode, allowAmbiguous);
                    break;
                case Lexer.Function:
                    expression = MethodNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.PrefixNode:
                case Lexer.SuffixNode:
                    expression = UnaryOperatorNode.Parse(parser, parent, lexerNode, allowAmbiguous);
                    break;
                case Lexer.FunctionCall:
                    expression = MethodCallNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.Assignment:
                    expression = AssignmentOperatorNode.Parse(parser, parent, lexerNode);
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (expression.TypeWrapper == null && !allowAmbiguous)
                throw new ParseException(expression.SequencePoint, "Ambiguous expression {0}", expression);
            return expression;
        }
        public override string ToString()
        {
            return String.Format("(ExpressionNode: {0} {1})", ExpressionType, ExpressionReturnType);
        }
    }
}
