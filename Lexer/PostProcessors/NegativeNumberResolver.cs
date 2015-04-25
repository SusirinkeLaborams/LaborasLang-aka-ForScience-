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
            if(astNode.Type == TokenType.PrefixNode && astNode.Children[1].Type == TokenType.Minus && astNode.Children[0].Type == TokenType.LiteralNode)
            {
                var literal = astNode.Children[0];
                var child = literal.Children[0];
                switch (child.Type)
                {
                    case TokenType.Float:
                    case TokenType.Integer:
                    case TokenType.Double:
                    case TokenType.Long:
                        child.Content = "-" + child.Content;
                        child.Node.Start = new Location(child.Node.Start.Column - 1, child.Node.Start.Row);
                        astNode.ReplaceWith(literal);
                        break;
                }
            }
        }
    }
}
