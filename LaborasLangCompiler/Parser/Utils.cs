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
        public static bool IsFunctionDeclaration(this AstNode node)
        {
            return node.Type == Lexer.TokenType.Value && node.Children[0].Type == Lexer.TokenType.Function;
        }

        public static string GetSingleSymbolOrThrow(this AstNode node)
        {
            if (node.Type == Lexer.TokenType.Symbol)
                return node.Content.ToString();

            if (node.Type == Lexer.TokenType.FullSymbol && node.ChildrenCount == 1)
                return node.Children[0].Content.ToString();

            throw new InvalidOperationException("Node not a single symbol node");
        }

        public static IEnumerator<AstNode> GetEnumerator(this AstNode node)
        {
            return node.Children.GetEnumerator();
        }
    }
}
