using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class ValueResolver : BottomUpPostProcessor
    {
        private static TokenType[][] c_OperatorGroups = new[]{
        #region period
            new[]{TokenType.Period},
        #endregion
        #region assignments
            new[]{TokenType.Assignment, TokenType.PlusEqual, TokenType.MinusEqual, TokenType.DivideEqual, TokenType.MultiplyEqual, TokenType.RemainderEqual, TokenType.LeftShiftEqual, TokenType.RightShiftEqual, TokenType.LogicalAndEqual, TokenType.LogicalOrEqual, TokenType.BitwiseAndEqual, TokenType.BitwiseXorEqual, TokenType.BitwiseOrEqual },
        #endregion
        #region shifts
            new[]{TokenType.LeftShift, TokenType.RightShift },
        #endregion
        #region addition
            new[]{TokenType.Plus, TokenType.Minus},
        #endregion
        #region multiplication
            new[]{TokenType.Multiply, TokenType.Divide, TokenType.Remainder},
        #endregion
        #region bitwise
            new[]{TokenType.BitwiseAnd, TokenType.BitwiseOr, TokenType.BitwiseXor, TokenType.BitwiseComplement},
        #endregion
 
        #region prefixes
            new[]{TokenType.PrefixOperator},
        #endregion
        #region postfixes
            new[]{TokenType.PostfixOperator},
        #endregion     
        #region comparison
            new[]{TokenType.Equal, TokenType.NotEqual, TokenType.More, TokenType.Less, TokenType.MoreOrEqual, TokenType.LessOrEqual},
        #endregion
        #region logical
            new[]{TokenType.LogicalOr, TokenType.LogicalAnd},
        #endregion
        
        };
        private int[] m_Priorities;

        public ValueResolver()
        {
            m_Priorities = Enumerable.Repeat(int.MaxValue, (int) TokenType.TokenTypeCount).ToArray();
           
            for (int i = 0; i < c_OperatorGroups.Length; i++)
            {
                foreach (var op in c_OperatorGroups[i])
                {
                    m_Priorities[(int)op] = i;
                }
            }
        }

        private int Priority(AbstractSyntaxTree type)
        {
            var parentPriority = m_Priorities[(int)type.Type];
            var childPriority = m_Priorities[(int)type.Children[0].Type];

            return parentPriority < childPriority ? parentPriority : childPriority;
        }

        public override void Transform(AbstractSyntaxTree astNode)
        {
            if (astNode.Type == TokenType.Value)
            {
                var tree = new AbstractSyntaxTree(new Node(TokenType.InfixNode), new List<AbstractSyntaxTree>());

                var stack = new Stack<AbstractSyntaxTree>();
                foreach (var item in astNode.Children)
                {
                    switch(item.Type)
                    {
                        case TokenType.PrefixOperator:
                            stack.Push(item);
                            break;
                        case TokenType.InfixOperator:
                        case TokenType.PostfixOperator:
                             while (stack.Count != 0 && ShouldPop(stack, item))
                            {
                                FormSubnode(tree, stack.Pop());
                            }
                            stack.Push(item);
                            break;
                        default:
                            if (stack.Count != 0 && stack.Peek().Type == TokenType.PrefixOperator)
                            {
                                tree.Children.Add(item);
                                while (stack.Count != 0 && stack.Peek().Type == TokenType.PrefixOperator)
                                {
                                    FormSubnode(tree, stack.Pop(), 1, TokenType.PrefixNode);
                                }
                            } 
                            else
                            {
                                Append(tree, item);
                            }
                            break;
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
            switch (value.Type)
            {
                case TokenType.InfixOperator:
                    FormSubnode(tree, value, 2, TokenType.InfixNode);
                    return;
                case TokenType.PostfixOperator:
                    FormSubnode(tree, value, 1, TokenType.PostfixNode);
                    return;
                default:
                    throw new ArgumentException();
            }
        }

        private static void FormSubnode(AbstractSyntaxTree tree, AbstractSyntaxTree value, int tokensToConsume, TokenType nodetype)
        {
            var source = tree.Children.GetRange(tree.Children.Count - tokensToConsume, tokensToConsume);
            tree.Children.RemoveRange(tree.Children.Count - tokensToConsume, tokensToConsume);
            source.Add(value);

            tree.Children.Add(new AbstractSyntaxTree(new Node(nodetype), source));
            
        }
        private bool ShouldPop(Stack<AbstractSyntaxTree> stack, AbstractSyntaxTree item)
        {            
            if (item.Type.IsRightAssociative())
            {
                return Priority(stack.Peek()) < Priority(item);
            }
            else
            {
                return Priority(stack.Peek()) >= Priority(item);
            }
        }
    }
}
