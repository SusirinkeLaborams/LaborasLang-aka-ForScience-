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
using LaborasLangCompiler.ILTools;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ExpressionNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract bool IsGettable { get; }
        public abstract bool IsSettable { get; }
        protected ExpressionNode(SequencePoint sequencePoint) : base(sequencePoint) { }
        public static ExpressionNode Parse(Parser parser, Context parent, AstNode lexerNode, TypeReference expectedType = null)
        {
            ExpressionNode ret = null;
            switch (lexerNode.Type)
            {
                case Lexer.TokenType.PeriodNode:
                case Lexer.TokenType.FullSymbol:
                    ret = DotOperatorNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.Symbol:
                    ret = SymbolNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.LiteralNode:
                    ret = LiteralNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.Value:
                    ret = ExpressionNode.Parse(parser, parent, lexerNode.Children[0], expectedType);
                    break;
                case Lexer.TokenType.Function:
                    ret = MethodNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.AssignmentOperatorNode:
                    ret = AssignmentOperatorNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.FunctionCallNode:
                    ret = MethodCallNode.Parse(parser, parent, lexerNode);
                    break;
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
                    ret = BinaryOperatorNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.PostfixNode:
                case Lexer.TokenType.PrefixNode:
                    ret = UnaryOperatorNode.Parse(parser, parent, lexerNode);
                    break;
                case Lexer.TokenType.ParenthesesNode:
                    ret = ExpressionNode.Parse(parser, parent, lexerNode.Children[1], expectedType);
                    break;
                default:
                    throw new NotImplementedException();
            }
            if(!(expectedType == null || expectedType.IsAuto()))
            {
                var ambiguous = ret as AmbiguousNode;
                if(ambiguous != null)
                {
                    if(ambiguous.ExpressionReturnType == null || 
                        ambiguous.ExpressionReturnType.IsAssignableTo(expectedType))
                    {
                        ret = ambiguous.RemoveAmbiguity(parser, expectedType);
                    }
                }
            }

            return ret;
        }
        public override string ToString()
        {
            return String.Format("(ExpressionNode: {0} {1})", ExpressionType, ExpressionReturnType);
        }
    }
}
