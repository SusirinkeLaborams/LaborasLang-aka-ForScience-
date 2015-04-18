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

            //init
            if(astNode.Children[2].Type == TokenType.EndOfLine)
            {
                astNode.Children.Insert(2, new AbstractSyntaxTree(new Node(TokenType.Empty), new List<AbstractSyntaxTree>()));
            }

            //condition
            if(astNode.Children[4].Type == TokenType.EndOfLine)
            {
                astNode.Children.Insert(4, new AbstractSyntaxTree(new Node(TokenType.Empty), new List<AbstractSyntaxTree>()));
            }

            //increment
            if (astNode.Children[6].Type == TokenType.RightParenthesis)
            {
                astNode.Children.Insert(6, new AbstractSyntaxTree(new Node(TokenType.Empty), new List<AbstractSyntaxTree>()));
            }
        }
    }
}
