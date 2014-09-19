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
                case Lexer.TokenType.PeriodNode:
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
                case Lexer.TokenType.AssignmentOperatorNode:
                case Lexer.TokenType.OrNode:
                case Lexer.TokenType.AndNode:
                case Lexer.TokenType.BitwiseOrNode:
                case Lexer.TokenType.BitwiseXorNode:
                case Lexer.TokenType.BitwiseAndNode:
                case Lexer.TokenType.NotEqualNode:
                case Lexer.TokenType.EqualNode:
                case Lexer.TokenType.LessOrEqualNode:
                case Lexer.TokenType.MoreOrEqualNode:
                case Lexer.TokenType.LessNode:
                case Lexer.TokenType.MoreNode:
                case Lexer.TokenType.RightShiftNode:
                case Lexer.TokenType.LeftShiftNode:
                case Lexer.TokenType.MinusNode:
                case Lexer.TokenType.PlusNode:
                case Lexer.TokenType.RemainderNode:
                case Lexer.TokenType.DivisionNode:
                case Lexer.TokenType.MultiplicationNode:
                    expression = BinaryOperatorNode.Parse(parser, parent, lexerNode, allowAmbiguous);
                    break;
                case Lexer.TokenType.PostfixNode:
                case Lexer.TokenType.PrefixNode:
                    expression = UnaryOperatorNode.Parse(parser, parent, lexerNode, allowAmbiguous);
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
