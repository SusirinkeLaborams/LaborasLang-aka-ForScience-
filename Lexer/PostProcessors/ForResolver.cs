using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class ForResolver : PostProcessor
    {
        public override void Transform(AbstractSyntaxTree astNode)
        {
            bool isFor = astNode.Type == TokenType.ForLoop && !astNode.Children.Any(child => child.Type == TokenType.In);
            if(!isFor)
            {
                return;
            }

            var filled = new List<AbstractSyntaxTree>();
            foreach (var node in astNode.Children)
            {
                //for, left parenthesis
                if (filled.Count < 2)
                {
                    filled.Add(node);
                    continue;
                }


            }
        }
    }
}
