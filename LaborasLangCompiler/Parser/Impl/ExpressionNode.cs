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

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ExpressionNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ReturnType { get; set; }
        public static new ExpressionNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            switch (lexerNode.Token.Name)
            {
                case Lexer.Symbol:
                    return LValueNode.Parse(parser, parentClass, parentBlock, lexerNode);
                case Lexer.Literal:
                    return LiteralNode.Parse(parser, parentClass, parentBlock, lexerNode);
                case Lexer.Value:
                    return ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
                case Lexer.Sum:
                case Lexer.Product:
                    return BinaryOperatorNode.Parse(parser, parentClass, parentBlock, lexerNode);
                case Lexer.Function:
                   return FunctionDeclarationNode.Parse(parser, parentClass, parentBlock, lexerNode);
                default:
                    throw new NotImplementedException();
            }
        }
        public override string Print()
        {
            return String.Format("(ExpressionNode: {0} {1})", ExpressionType, ReturnType);
        }
    }
}
