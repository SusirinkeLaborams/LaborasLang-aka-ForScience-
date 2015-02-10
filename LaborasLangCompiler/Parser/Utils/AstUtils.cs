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
        public static bool IsFunctionDeclaration(this AstNode node)
        {
            if (node.Type == Lexer.TokenType.Function)
                return true;

            if (node.Type == Lexer.TokenType.Value)
                return node.Children[0].IsFunctionDeclaration();

            return false;
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
