using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class ValuePostProcessor : PostProcessor
    {

        ValuePostProcessor(RootNode root) : base(root)
        {

        }
         public override void Transform(RootNode RootNode, AstNode astNode)
        {
            if (astNode.Type == TokenType.RawOperatorNode)
            {
                var queue = new Queue<AstNode>();
                var stack = new Stack<AstNode>();

                for (int i = 0; i < astNode.ChildrenCount; i++)
                {
                    var child = astNode.Children[i].Priority();
                }
            }
        }

        
    }

    static class AstNodeExtensions
    {
        private static TokenType[] OperatorPriorityList = new[] {TokenType.LeftParenthesis, TokenType.RightParenthesis, TokenType.Period, TokenType.PostfixOperator, TokenType.PrefixOperator, TokenType.MultiplicativeOperator, TokenType.AdditiveOperator, TokenType.ShiftOperator,
                        TokenType.RelationalOperator, TokenType.EqualityOperator, TokenType.BitwiseAnd, TokenType.BitwiseXor, TokenType.BitwiseOr,
                        TokenType.LogicalAnd, TokenType.LogicalOr, TokenType.AssignmentOperator};
      
        public static int Priority(this AstNode node)
        {
            throw new NotImplementedException();
        }
    }
}
