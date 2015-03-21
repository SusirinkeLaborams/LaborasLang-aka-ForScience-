using Lexer.Containers;
using System;

namespace Lexer
{
    public sealed class Lexer
    {
        public static RootNode Lex(string source)
        {
            var rootNode = new RootNode();
            var tokens = Tokenizer.Tokenize(source, rootNode);
            var matcher = new SyntaxMatcher(tokens, rootNode);
            matcher.Match();

            return rootNode;
        }

        public static void WithTree(string source, Action<AstNode> consumer)
        {
            using (var rootNode = new RootNode())
            {
                var tokens = Tokenizer.Tokenize(source, rootNode);
                var matcher = new SyntaxMatcher(tokens, rootNode);
                consumer.Invoke(matcher.Match());
            }
        }
    }
}
