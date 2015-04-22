using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class NegativeNumberResolver : PostProcessor
    {
        public override void Transform(AbstractSyntaxTree astNode)
        {
            if(astNode.Type == TokenType.PrefixNode && astNode.Children[1].Type == TokenType.Minus)
            {

                switch (astNode.Children[0].Type)
                {
                    case TokenType.Float:
                    case TokenType.Integer:
                    case TokenType.Double:
                    case TokenType.Long:
                        astNode.ReplaceWith(astNode.Children[0]);
                        astNode.Content = "-" + astNode.Content;
                        astNode.Node.Start = new Location(astNode.Node.Start.Column - 1, astNode.Node.Start.Row);
                        break;
                }
            }
        }
    }
}
