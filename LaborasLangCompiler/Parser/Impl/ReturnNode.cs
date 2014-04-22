using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ReturnNode : ParserNode, IReturnNode
    {
        public override NodeType Type { get { return NodeType.ReturnNode; } }
        public IExpressionNode Expression { get; private set; }
        public static ReturnNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new ReturnNode();
            if(lexerNode.Children.Count > 0)
                instance.Expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(ReturnNode: {0})", Expression);
        }
    }
}
