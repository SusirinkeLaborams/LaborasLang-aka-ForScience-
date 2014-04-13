using LaborasLangCompiler.Parser.Tree;
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
        public abstract IExpressionNode RightOperand { get; }
        public abstract IExpressionNode LeftOperand { get; }
        public abstract BinaryOperatorNodeType BinaryOperatorType { get; }
        public static new ExpressionNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            else
                throw new NotImplementedException("Not parsing binary operators");
        }
    }
}
