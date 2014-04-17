using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class BinaryOperatorNode : RValueNode, IBinaryOperatorNode
    {
        public IExpressionNode RightOperand { get; set; }
        public IExpressionNode LeftOperand { get; set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.BinaryOperator; } }
        public BinaryOperatorNodeType BinaryOperatorType { get; set; }
        public override TypeReference ReturnType { get; set; }
        public static new ExpressionNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
            {
                return ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            }
            else
            {
                switch (lexerNode.Token.Name)
                {
                    case Lexer.Sum:
                    case Lexer.Product:
                        return ArithmeticOperatorNode.Parse(parser, parentClass, parentBlock, lexerNode);
                    default:
                        throw new NotImplementedException("Only parsing Sum and Product");
                }
            }
        }
    }
}
