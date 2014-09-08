using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Operators
{
    class TemporaryOperatorParser
    {
        public static ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if(lexerNode.ChildrenCount == 1)
            {
                return ExpressionNode.Parse(parser, parent, lexerNode.Children[0].Children[0]);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
