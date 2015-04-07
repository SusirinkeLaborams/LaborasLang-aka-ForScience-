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
                    var old = astNode.Children;
                    int count = old.Count;

                    var head = old.Take(count - 2);
                    var dot = old[count - 2];
                    var tail = old[count - 1];

                    var headNode = head.Count() > 1 ? new AbstractSyntaxTree(new Node(TokenType.FullSymbol), head.ToList()) : head.First();

                    astNode.Children = new List<AbstractSyntaxTree>() { headNode, tail, dot };
                }
            }
        }
    }
}
