using LaborasLangCompiler.Parser.Exceptions;
using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    static class Utils
    {
        public static bool IsSettable(this IExpressionNode node)
        {
            if (node.ExpressionType != ExpressionNodeType.LValue)
                return false;
            return true;
            //properties vistiek dar neveikia
        }

        public static bool IsGettable(this IExpressionNode node)
        {
            //kol kas viskas gettable
            return true;
        }

        public static int Count(this AstNodeList list, Predicate<AstNode> pred)
        {
            int count = 0;
            foreach(var node in list)
            {
                if(pred(node))
                {
                    count++;
                }
            }
            return count;
        }

        public static bool IsFunctionDeclaration(this AstNode node)
        {
            if(node.Type == Lexer.TokenType.DeclarationNode)
            {
                return node.Children.Any(n => n.Type == Lexer.TokenType.Function);
            }
            else
            {
                return false;
            }
        }

        public static string GetSingleSymbolOrThrow(this AstNode node)
        {
            if (node.Type == Lexer.TokenType.Symbol)
                return node.Content.ToString();

            if (node.Type == Lexer.TokenType.FullSymbol && node.ChildrenCount == 1)
                return node.Children[0].Content.ToString();

            throw new InvalidOperationException("Node not a single symbol node");
        }
    }
}
