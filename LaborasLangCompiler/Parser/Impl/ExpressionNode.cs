using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

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
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.FullSymbol:
                    expression = DotOperatorNode.Parse(parser, parent, lexerNode).ExtractExpression(allowAmbiguous);
                    break;
                case Lexer.TokenType.Symbol:
                    expression = SymbolNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.StringLiteral:
                case Lexer.TokenType.Float:
                case Lexer.TokenType.Double:
                case Lexer.TokenType.Integer:
                case Lexer.TokenType.Long:
                case Lexer.TokenType.True:
                case Lexer.TokenType.False:
                    expression = LiteralNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.LValue:
                case Lexer.TokenType.RValue:
                case Lexer.TokenType.Value:
                    expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0], allowAmbiguous);
                    break;
                case Lexer.TokenType.Function:
                    expression = MethodNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.FunctionCall:
                    expression = MethodCallNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.AssignmentNode:
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
