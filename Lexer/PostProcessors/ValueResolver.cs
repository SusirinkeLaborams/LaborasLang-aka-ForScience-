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
        
        #region assignments
            new[]{TokenType.Assignment, TokenType.PlusEqual, TokenType.MinusEqual, TokenType.DivideEqual, TokenType.MultiplyEqual, TokenType.RemainderEqual, TokenType.LeftShiftEqual, TokenType.RightShiftEqual, TokenType.LogicalAndEqual, TokenType.LogicalOrEqual, TokenType.BitwiseAndEqual, TokenType.BitwiseXorEqual, TokenType.BitwiseOrEqual },
        #endregion

        #region logical
            new[]{TokenType.LogicalOr, TokenType.LogicalAnd},
        #endregion

        #region comparison
            new[]{TokenType.Equal, TokenType.NotEqual, TokenType.More, TokenType.Less, TokenType.MoreOrEqual, TokenType.LessOrEqual},
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

         #region functionCall, ArrayAccess
            new[]{TokenType.FunctionArgumentsList, TokenType.IndexNode, TokenType.Period},
        #endregion
 
        };

        private readonly int[] m_Priorities;

        public ValueResolver()
        {
            m_Priorities = Enumerable.Repeat(int.MinValue, (int) TokenType.TokenTypeCount).ToArray();
           
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
            var childPriority = parentPriority;
            if (type.Children.Count > 0)
            {
                childPriority = m_Priorities[(int)type.Children[0].Type];
            }
            
            return Math.Max(childPriority, parentPriority);
        }

        public override void Transform(AbstractSyntaxTree astNode)
        {
            if (astNode.Type == TokenType.Value)
            {
                var tree = new AbstractSyntaxTree(new Node(TokenType.Value), new List<AbstractSyntaxTree>());

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
                            Append(tree, item);
                            break;
                    }
                }

                while (stack.Count != 0)
                {
                    FormSubnode(tree, stack.Pop());
                }

                Contract.Assume(tree.Children.Count == 1);
                astNode.ReplaceWith(tree);
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
                case TokenType.PrefixOperator:
                    FormSubnode(tree, value, 1, TokenType.PrefixNode);
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

            var node = new Node(nodetype);
            node.Start = source.First().Node.Start;
            node.End = source.Last().Node.End;

            tree.Children.Add(new AbstractSyntaxTree(node, source));         
        }
        private bool ShouldPop(Stack<AbstractSyntaxTree> stack, AbstractSyntaxTree item)
        {            
            if (IsRightAssociative(item))
            {
                return Priority(stack.Peek()) < Priority(item);
            }
            else
            {
                return Priority(stack.Peek()) >= Priority(item);
            }
        }

        private static bool IsRightAssociative(AbstractSyntaxTree tree)
        {
            if (tree.Type.IsRightAssociative())
            {
                return true;
            } 
            else
            {
                if (tree.Children.Count == 1)
                {
                    return IsRightAssociative(tree.Children[0]);
                }
            }
            return false;
        }

    }
}
