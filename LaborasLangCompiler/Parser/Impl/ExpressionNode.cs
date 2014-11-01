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
        public abstract bool IsGettable { get; }
        public abstract bool IsSettable { get; }
        protected ExpressionNode(SequencePoint sequencePoint) : base(sequencePoint) { }
        public static ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.PeriodNode:
                case Lexer.TokenType.FullSymbol:
                    return DotOperatorNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.Symbol:
                    return SymbolNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.LiteralNode:
                    return LiteralNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.Value:
                    return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                case Lexer.TokenType.Function:
                    return MethodNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.AssignmentOperatorNode:
                    return AssignmentOperatorNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.FunctionCallNode:
                    return MethodCallNode.Parse(parser, parent, lexerNode);
                case Lexer.TokenType.LogicalOrNode:
                case Lexer.TokenType.LogicalAndNode:
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
                case Lexer.TokenType.ParenthesesNode:
                    return ExpressionNode.Parse(parser, parent, lexerNode.Children[1]);
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
