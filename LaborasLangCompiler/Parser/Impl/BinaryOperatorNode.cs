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
        public IExpressionNode RightOperand { get; set; }
        public IExpressionNode LeftOperand { get; set; }
        public abstract BinaryOperatorNodeType BinaryOperatorType { get; }
        public static new ExpressionNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            if (lexerNode.Children.Count == 1)
                return ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            else
                throw new NotImplementedException("Not parsing binary operators");
        }
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is BinaryOperatorNode))
                return false;
            var that = (BinaryOperatorNode)obj;
            return base.Equals(obj) && BinaryOperatorType == that.BinaryOperatorType && RightOperand.Equals(that.RightOperand) && LeftOperand.Equals(that.LeftOperand);
        }
    }
}
