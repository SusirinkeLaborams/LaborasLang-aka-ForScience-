using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;

namespace LaborasLangCompiler.Parser.Impl
{
    class ReturnNode : ParserNode, IReturnNode
    {
        public override NodeType Type { get { return NodeType.ReturnNode; } }
        public IExpressionNode Expression { get; private set; }
        public static ReturnNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new ReturnNode();
            if (lexerNode.Children.Count > 0)
            {
                instance.Expression = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
                var functionReturn = parent.GetFunction().FunctionReturnType;
                if (!instance.Expression.ReturnType.IsAssignableTo(functionReturn))
                    throw new TypeException(String.Format("Function returns {0}, cannot return {1}",
                        functionReturn, instance.Expression.ReturnType));
            }
            return instance;
        }
        public override string ToString()
        {
            return String.Format("(ReturnNode: {0})", Expression);
        }
    }
}
