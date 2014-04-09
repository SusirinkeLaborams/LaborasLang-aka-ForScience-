using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class ExpressionNode : ParserNode, IExpressionNode
    {
        public override NodeType Type { get { return NodeType.Expression; } }
        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ReturnType { get; protected set; }

        public static new ExpressionNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            switch(lexerNode.Token.Name)
            {
                case "Symbol":
                    return LValueNode.Parse(parser, parent, lexerNode);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
