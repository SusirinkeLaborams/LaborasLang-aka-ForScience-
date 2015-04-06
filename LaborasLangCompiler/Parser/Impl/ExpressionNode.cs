using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using LaborasLangCompiler.Codegen;
using Lexer;
using LaborasLangCompiler.Parser.Impl.Operators;

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

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode, TypeReference expectedType = null)
        {
            ExpressionNode ret = null;
            switch (lexerNode.Type)
            {
                //case Lexer.TokenType.PeriodNode:
                case Lexer.TokenType.FullSymbol:
                    ret = DotOperatorNode.Parse(context, lexerNode);
                    break;
                case Lexer.TokenType.Symbol:
                    ret = SymbolNode.Parse(context, lexerNode);
                    break;
                case Lexer.TokenType.LiteralNode:
                    ret = LiteralNode.Parse(context, lexerNode);
                    break;
                case Lexer.TokenType.Value:
                    ret = ExpressionNode.Parse(context, lexerNode.Children[0], expectedType);
                    break;
                case Lexer.TokenType.Function:
                    ret = MethodNode.Parse(context, lexerNode);
                    break;
                case Lexer.TokenType.InfixNode:
                    ret = InfixParser.Parse(context, lexerNode);
                    break;
                case Lexer.TokenType.PostfixNode:
                case Lexer.TokenType.PrefixNode:
                    ret = UnaryOperators.Parse(context, lexerNode);
                    break;
                case Lexer.TokenType.ParenthesesNode:
                    ret = ExpressionNode.Parse(context, lexerNode.Children[1], expectedType);
                    break;
                case Lexer.TokenType.ArrayLiteral:
                    ret = ArrayCreationNode.Parse(context, lexerNode);
                    break;
                default:
                    throw new NotImplementedException();
            }
            if(!(expectedType == null || expectedType.IsAuto()))
            {
                var ambiguous = ret as IAmbiguousNode;
                if(ambiguous != null)
                {
                    ret = ambiguous.RemoveAmbiguity(context, expectedType);
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
