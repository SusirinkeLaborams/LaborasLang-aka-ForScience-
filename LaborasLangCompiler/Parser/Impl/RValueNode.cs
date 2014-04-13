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
    }
}
