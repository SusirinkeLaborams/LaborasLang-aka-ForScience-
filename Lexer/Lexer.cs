using Lexer.Containers;

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
    }
}
