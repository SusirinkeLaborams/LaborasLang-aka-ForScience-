using LaborasLangCompiler.Parser.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class RValueNode : ExpressionNode, IRValueNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.RValue; } }
        public abstract RValueNodeType RValueType { get; }
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is RValueNode))
                return false;
            var that = (RValueNode)obj;
            return base.Equals(obj) && RValueType == that.RValueType;
        }
    }
}
