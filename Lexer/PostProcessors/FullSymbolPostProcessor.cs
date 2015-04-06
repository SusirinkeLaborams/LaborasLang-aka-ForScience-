using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class FullSymbolPostProcessor : PostProcessor
    {
        public override void Transform(AbstractSyntaxTree astNode)
        {
            if (astNode.Type == TokenType.FullSymbol)
            {
                if (astNode.Children.Count == 1)
                {
                    return;
                }
                else
                {
                    var symbol = astNode.Children.First();
                    
                    var source = astNode.Children;
                    var remainder = new AbstractSyntaxTree(new Node(TokenType.FullSymbol), source.GetRange(2, source.Count - 2));
                    
                    astNode.Children = new List<AbstractSyntaxTree>() { symbol, remainder };
                }
            }
        }
    }
}
