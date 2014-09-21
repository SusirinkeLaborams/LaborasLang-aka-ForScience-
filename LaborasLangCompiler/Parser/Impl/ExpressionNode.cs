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
        public static ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.PeriodNode:
                    return DotOperatorNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.Symbol:
                    return SymbolNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.StringLiteral:
                case Lexer.TokenType.Float:
                case Lexer.TokenType.Double:
                case Lexer.TokenType.Integer:
                case Lexer.TokenType.Long:
                case Lexer.TokenType.True:
                case Lexer.TokenType.False:
                    return LiteralNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.Value:
                    return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                case Lexer.TokenType.Function:
                    return MethodNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.AssignmentOperatorNode:
                case Lexer.TokenType.OrNode:
                case Lexer.TokenType.AndNode:
                case Lexer.TokenType.BitwiseOrNode:
                case Lexer.TokenType.BitwiseXorNode:
                case Lexer.TokenType.BitwiseAndNode:
                case Lexer.TokenType.EqualityOperatorNode:
                case Lexer.TokenType.RelationalOperatorNode:
                case Lexer.TokenType.ShiftOperatorNode:
                case Lexer.TokenType.AdditiveOperatorNode:
                case Lexer.TokenType.MultiplicativeOperatorNode:
                    return BinaryOperatorNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.PostfixNode:
                case Lexer.TokenType.PrefixNode:
                    return UnaryOperatorNode.Parse(parser, parent, lexerNode);
                default:
                    throw new NotImplementedException();
            }
        }
        public override string ToString()
        {
            return String.Format("(ExpressionNode: {0} {1})", ExpressionType, ExpressionReturnType);
        }
    }
}
