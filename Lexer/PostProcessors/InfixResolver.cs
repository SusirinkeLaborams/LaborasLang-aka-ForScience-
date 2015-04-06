using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class InfixResolver : BottomUpPostProcessor
    {
        private TokenType[] m_PrioritizedOperators;

        public InfixResolver()
        {
            var operators = RulePool.LaborasLangRuleset.First(rule => rule.Result == TokenType.InfixOperator).RequiredTokens;
            m_PrioritizedOperators = operators.Select(op => op[0].Token).ToArray();
        }

        private int Priority(AbstractSyntaxTree type)
        {
            return Array.IndexOf(m_PrioritizedOperators, type.Children[0].Type);
        }

        public override void Transform(AbstractSyntaxTree astNode)
        {
            if (astNode.Type == TokenType.InfixNode)
            {
                var tree = new AbstractSyntaxTree(new Node(TokenType.InfixNode), new List<AbstractSyntaxTree>());

                var stack = new Stack<AbstractSyntaxTree>();
                foreach (var item in astNode.Children)
                {
                    if (item.Type == TokenType.InfixOperator)
                    {
                        while (stack.Count != 0 && ShouldPop(stack, item))
                        {
                            FormSubnode(tree, stack.Pop());
                        }
                        stack.Push(item);
                    }
                    else
                    {
                        Append(tree, item);
                    }
                }

                while (stack.Count != 0)
                {
                    FormSubnode(tree, stack.Pop());
                }

                Contract.Assume(tree.Children.Count == 1);
                astNode.ReplaceWith(tree.Children[0]);
            }
        }

        private static void Append(AbstractSyntaxTree tree, AbstractSyntaxTree value)
        {
            tree.Children.Add(value);
        }

        private static void FormSubnode(AbstractSyntaxTree tree, AbstractSyntaxTree value)
        {
            var source = tree.Children.GetRange(tree.Children.Count - 2, 2);
            tree.Children.RemoveRange(tree.Children.Count - 2, 2);
            source.Add(value);

            tree.Children.Add(new AbstractSyntaxTree(new Node(TokenType.InfixNode), source));
            
        }
        private bool ShouldPop(Stack<AbstractSyntaxTree> stack, AbstractSyntaxTree item)
        {
            if (item.Children[0].Type.IsRightAssociative())
            {
                return Priority(stack.Peek()) < Priority(item);
            }
            else
            {
                return Priority(stack.Peek()) > Priority(item);
            }
        }
    }
}
