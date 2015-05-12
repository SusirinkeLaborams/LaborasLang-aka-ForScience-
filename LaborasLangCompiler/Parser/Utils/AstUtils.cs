using Lexer;
using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Utils
{
    static class AstUtils
    {
        public static bool IsFunctionDeclaration(this IAbstractSyntaxTree node)
        {
            if (node.Type == Lexer.TokenType.Function)
                return true;

            if (node.Type == Lexer.TokenType.Value)
                return node.Children[0].IsFunctionDeclaration();

            return false;
        }

        public static string GetSingleSymbolOrThrow(this IAbstractSyntaxTree node)
        {
            if (node.Type == Lexer.TokenType.Symbol)
                return node.Content.ToString();

            if (node.Type == Lexer.TokenType.FullSymbol && node.Children.Count == 1)
                return node.Children[0].Content.ToString();

            throw new InvalidOperationException("Node not a single symbol node");
        }

        public static string GetFullSymbolTextContent(this IAbstractSyntaxTree node)
        {
            if (node.Type == TokenType.FullSymbol && node.Children.Count > 1)
            {
                return String.Format("{0}.{1}", 
                    node.Children[0].GetFullSymbolTextContent(), node.Children[1].GetFullSymbolTextContent());
            }

            if(node.Type == TokenType.FullSymbol)
            {
                return node.Children[0].GetFullSymbolTextContent();
            }

            if(node.Type == TokenType.Symbol)
            {
                return node.Content;
            }

            throw new ArgumentException(String.Format("Unexpected node type {0}", node.Type));
        }
    }
}
