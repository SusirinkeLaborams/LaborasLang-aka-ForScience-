using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lexer.PostProcessors
{
    class PostfixResolver : PostProcessor
    {
        public override void Transform(AbstractSyntaxTree astNode)
        {
            if (astNode.Type == TokenType.PostfixNode)
            {
                if (astNode.Children.Count == 1)
                {
                    astNode.Collapse();
                    return;
                }
                else
                {
                    var source = astNode.Children;
                    var value = new AbstractSyntaxTree(new Node(TokenType.PostfixNode), source.GetRange(0, source.Count - 1));
                    var postfix = source.Last(); 
                    astNode.Children = new List<AbstractSyntaxTree>() { value, postfix };
                }
            }
        }
    }
}
